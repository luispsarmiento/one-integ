using MongoDB.Bson;
using MongoDB.Bson.IO;
using OneInteg.Server.DataAccess;
using OneInteg.Server.Domain.Repositories;
using OneInteg.Server.Domain.Services;
using System.Net.Mail;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OneInteg.Server.Services
{
    public partial class SubscriptionService : BaseService<Subscription>, ISubscriptionService
    {
        protected readonly ISubscriptionRepository repository;

        protected readonly ICustomerRepository customerRepository;
        protected readonly IPlanRepository planRepository;
        public SubscriptionService(ISubscriptionRepository repository, ICustomerRepository customerRepository, IPlanRepository planRepository) : base(repository)
        {
            this.repository = repository;
            this.customerRepository = customerRepository;
            this.planRepository = planRepository;
        }

        public async Task<string> GetCheckoutUrl(Customer customer, string planReference, string promotionCode = "")
        {
            string checkoutUrl = "";
            try
            {
                if (!IsValidCustomer(customer.Email))
                {
                    return string.Empty;
                }

                var _customer = (await customerRepository.Find(doc => doc.Email == customer.Email &&
                                                                      doc.TenantId == customer.TenantId)).FirstOrDefault();

                if (_customer == null) 
                {
                    customer.CustomerId = Guid.NewGuid();
                    customer.CreateAt = DateTime.Now;
                    customer.UpdateAt = DateTime.Now;
                    await customerRepository.Add(customer);
                }

                var plan = (await planRepository.Find(doc => doc.PlanReference == planReference &&
                                                             doc.TenantId == customer.TenantId)).FirstOrDefault();

                if (!(plan is null || plan.Data is null))
                {
                    if (!string.IsNullOrEmpty(promotionCode))
                    {
                        var promotion = plan.Promotions.FirstOrDefault(p => p.Code == promotionCode);

                        // HACK: temporary workaround — expiration validation still pending
                        if (promotion is not null)
                        {
                            dynamic promotionData = GetDataObject(promotion.Data);
                            checkoutUrl = Convert.ToString(promotionData.init_point);
                        }
                    } 
                    else
                    {
                        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(plan.Data);
                        checkoutUrl = Convert.ToString(data.init_point);
                    }
                }

                return checkoutUrl;
            }
            catch (Exception ee)
            {
                throw;
            }
        }
        private bool IsValidCustomer(string customer)
        {
            try
            {
                var mailAddress = new MailAddress(customer);
                return true; // Format is valid
            }
            catch (FormatException)
            {
                return false; // Format is invalid
            }
        }

        private dynamic GetDataObject(string data)
        {
            dynamic dataObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(data);

            return dataObject;
        }

        public async Task<List<Subscription>> GetSubscriptionsSyncPending(DateTime date)
        {
            try
            {
                var queryResult = await repository.Find(doc => doc.NextPaymentDate <= date);

                return queryResult.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OneInteg.Server.Domain.Entities;
using OneInteg.Server.Domain.Repositories;
using System.Data;
using System.Text;
using System.Text.Json;

namespace OneInteg.Server.Services
{
    public class MercadoPagoPaymentProvider : IPaymentProvider
    {
        protected readonly ICustomerRepository _customerRepository;
        protected readonly ISubscriptionRepository _subscriptionRepository;

        protected string BaseUri;
        protected string AccessToken;
        public MercadoPagoPaymentProvider(ICustomerRepository customerRepository, ISubscriptionRepository subscriptionRepository, string baseUri, string accessToken)
        {
            this._customerRepository = customerRepository;
            this._subscriptionRepository = subscriptionRepository;

            this.BaseUri = baseUri;
            this.AccessToken = accessToken;
        }
        public async Task<Subscription?> HandleBackUrlSubscription(Guid tenantId, string preapprovalId, string customerEmail)
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(this.BaseUri);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.AccessToken}");

            using HttpResponseMessage response = await httpClient.GetAsync($"preapproval/{preapprovalId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            var _response = JsonConvert.DeserializeObject<PreapprovalResponse>(json);

            using var customerHttpClient = new HttpClient();
            customerHttpClient.BaseAddress = new Uri($"{this.BaseUri}v1/");
            customerHttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.AccessToken}");

            using HttpResponseMessage customerResponse = await customerHttpClient.GetAsync($"customers/{_response.payer_id}");
            string email = customerEmail;
            if (customerResponse.IsSuccessStatusCode && string.IsNullOrEmpty(email))
            {
                var customerJson = await customerResponse.Content.ReadAsStringAsync();
                var _customerResponse = JsonConvert.DeserializeObject<CustomerResponse>(customerJson);
                email = Convert.ToString(_customerResponse?.email ?? "");
            }

            var customer = this._customerRepository.Find(c => c.Email == email && c.TenantId == tenantId).Result.FirstOrDefault();

            if (customer == null) 
            {
                return null;
            }

            var subscription = new Subscription
            {
                SubscriptionId = Guid.NewGuid(),
                TenantId = customer.TenantId,
                CustomerId = customer.CustomerId,
                PaymentMethodId = _response.payment_method_id,
                Reference = preapprovalId,
                PlanReference = _response.preapproval_plan_id,
                StartDate = _response.auto_recurring.start_date,
                EndDate = _response.auto_recurring.end_date,
                NextPaymentDate = _response.next_payment_date,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
            };

            await this._subscriptionRepository.Add(new DataAccess.Subscription
            {
                SubscriptionId = subscription.SubscriptionId,
                TenantId = subscription.TenantId,
                CustomerId = subscription.CustomerId,
                PaymentMethodId = subscription.PaymentMethodId,
                Reference = subscription.Reference,
                PlanReference = subscription.PlanReference,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                NextPaymentDate = subscription.NextPaymentDate,
                CreateAt = subscription.CreateAt,
                UpdateAt = subscription.UpdateAt
            });

            return subscription;
        }

        public async Task<Subscription?> HandleSubscriptionPayment(Subscription preapproval)
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(this.BaseUri);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.AccessToken}");

            using HttpResponseMessage response = await httpClient.GetAsync($"preapproval/{preapproval.Reference}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var _response = JsonConvert.DeserializeObject<PreapprovalResponse>(json);

            if (_response == null || 
                _response.summarized.semaphore != MercadoPagoPreapprovalSemaphore.Green.Name)
            {
                return null;
            }

            preapproval.PaymentMethodId = _response.payment_method_id;
            preapproval.NextPaymentDate = _response.next_payment_date;
            preapproval.UpdateAt = DateTime.Now;

            await this._subscriptionRepository.Update(new DataAccess.Subscription
            {
                SubscriptionId = preapproval.SubscriptionId,
                TenantId = preapproval.TenantId,
                CustomerId = preapproval.CustomerId,
                PaymentMethodId = preapproval.PaymentMethodId,
                Reference = preapproval.Reference,
                PlanReference = preapproval.PlanReference,
                StartDate = preapproval.StartDate,
                EndDate = preapproval.EndDate,
                NextPaymentDate = preapproval.NextPaymentDate,
                CreateAt = preapproval.CreateAt,
                UpdateAt = preapproval.UpdateAt
            });

            return preapproval;
        }
    }

    class PreapprovalResponse
    {
        public string payer_id { get; set; }
        public string payment_method_id { get; set; }
        public string preapproval_plan_id { get; set; }
        public PreapprovalPeriodResponse auto_recurring { get; set; }
        public DateTime next_payment_date { get; set; }
        public PreapprovalSummaryResponse summarized { get; set; }
    }

    class PreapprovalPeriodResponse
    {
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
    }

    class CustomerResponse
    {
        public string email { get; set; }
    }

    public enum PaymentProviderType
    {
        MercadoPago
    }

    class PreapprovalSummaryResponse
    {
        public string semaphore { get; set; }
    }

    public record MercadoPagoPreapprovalSemaphore(string Name)
    {
        public static MercadoPagoPreapprovalSemaphore Green { get; } = new("green");
    }
}

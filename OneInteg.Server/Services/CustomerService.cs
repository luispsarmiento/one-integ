using OneInteg.Server.DataAccess;
using OneInteg.Server.Domain.Repositories;
using OneInteg.Server.Domain.Services;

namespace OneInteg.Server.Services
{
    public class CustomerService : BaseService<Customer>, ICustomerService
    {
        protected readonly ICustomerRepository repository;
        public CustomerService(ICustomerRepository repository) : base(repository)
        {
        }
    }
}

using OneInteg.Server.DataAccess;
using OneInteg.Server.Domain.Repositories;
using OneInteg.Server.Domain.Services;

namespace OneInteg.Server.Services
{
    public class TenantService : BaseService<Tenant>, ITenantService
    {
        protected readonly ITenantRepository repository;
        public TenantService(ITenantRepository repository) : base(repository)
        {
        }
    }
}

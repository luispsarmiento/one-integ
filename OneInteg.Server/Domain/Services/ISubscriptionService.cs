

using OneInteg.Server.DataAccess;

namespace OneInteg.Server.Domain.Services
{
    public interface ISubscriptionService : IBaseService<Subscription>
    {
        Task<string> GetCheckoutUrl(Customer customer, string planReference, string promotionCode = "");
    }
}

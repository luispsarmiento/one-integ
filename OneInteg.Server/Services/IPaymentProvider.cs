using OneInteg.Server.Domain.Entities;

namespace OneInteg.Server.Services;

public interface IPaymentProvider
{
    public Task<Subscription?> HandleBackUrlSubscription(Guid tenantId, string preapprovalId, string customerEmail);
    public Task<Subscription?> HandleSubscriptionPayment(Subscription preapproval);
}
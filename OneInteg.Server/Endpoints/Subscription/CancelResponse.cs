

using OneInteg.Server.Domain.Entities;

namespace OneInteg.Server.Endpoints.Subscription
{
    public record CancelResponse(
            string Id,
            Guid SubscriptionId,
            Guid TenantId,
            Guid CustomerId,
            string PaymentMethodId,
            string Reference,
            string PlanReference,
            DateTime StartDate,
            DateTime EndDate,
            DateTime NextPaymentDate,
            DateTime UpdateAt,
            string Status,
            DateTime? CancelledAt
        )
    {
        public static explicit operator CancelResponse(OneInteg.Server.DataAccess.Subscription source)
        {
            return new CancelResponse(
                    source._id.ToString(),
                    source.SubscriptionId,
                    source.TenantId,
                    source.CustomerId,
                    source.PaymentMethodId,
                    source.Reference,
                    source.PlanReference,
                    source.StartDate,
                    source.EndDate,
                    source.NextPaymentDate,
                    source.UpdateAt,
                    source.Status.ToString(),
                    source.CancelledAt
                );
        }
    }
}

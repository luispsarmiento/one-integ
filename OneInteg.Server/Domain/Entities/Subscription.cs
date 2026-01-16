namespace OneInteg.Server.Domain.Entities;

public class Subscription
{
    public Guid SubscriptionId { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public string PaymentMethodId { get; set; }
    public string Reference { get; set; }
    public string PlanReference { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime NextPaymentDate { get; set; }
    public DateTime UpdateAt { get; set; }
    public DateTime CreateAt { get; set; }
    public SubscriptionStatusEnum? Status { get; set; }
    public DateTime? CancelledAt { get; set; }
}
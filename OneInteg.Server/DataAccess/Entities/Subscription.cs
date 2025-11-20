using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OneInteg.Shared;

namespace OneInteg.Server.DataAccess
{
    public class Subscription : BaseEntity
    {
        [BsonRepresentation(BsonType.String)]
        public Guid SubscriptionId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid TenantId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid CustomerId { get; set; }
        public string PaymentMethodId { get; set; }
        public string Reference { get; set; }
        public string PlanReference { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public DateTime UpdateAt { get; set; }
        public DateTime CreateAt { get; set; }
    }
}

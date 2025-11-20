using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OneInteg.Shared;

namespace OneInteg.Server.DataAccess
{
    public class Customer : BaseEntity
    {
        [BsonRepresentation(BsonType.String)]
        public Guid CustomerId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid TenantId { get; set; }
        public string Email { get; set; }
        public DateTime UpdateAt { get; set; }
        public DateTime CreateAt { get; set; }
    }
}

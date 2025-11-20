using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OneInteg.Server.Domain.Entities;
using OneInteg.Shared;

namespace OneInteg.Server.DataAccess
{
    public class Tenant : BaseEntity
    {
        [BsonRepresentation(BsonType.String)]
        public Guid TenantId { get; set; }
        public string Name { get; set; }
        public TenantSettings Settings { get; set; }
        public DateTime UpdateAt { get; set; }
        public DateTime CreateAt { get; set; }
    }
}

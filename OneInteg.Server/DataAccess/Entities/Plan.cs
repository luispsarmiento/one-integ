using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OneInteg.Shared;

namespace OneInteg.Server.DataAccess
{
    public class Plan : BaseEntity
    {
        [BsonRepresentation(BsonType.String)]
        public Guid TenantId { get; set; }
        public string PlanReference { get; set; }
        public string Data { get; set; }
        public List<Promotion> Promotions { get; set; }
        public DateTime UpdateAt { get; set; }
        public DateTime CreateAt { get; set; }
    }

    public class Promotion
    {
        public string Code { get; set; }
        public string Data { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}

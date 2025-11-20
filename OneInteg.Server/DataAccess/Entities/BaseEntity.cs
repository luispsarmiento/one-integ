using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OneInteg.Server.DataAccess
{
    public class BaseEntity
    {
        [BsonId]
        public ObjectId _id { get; set; }
    }
}

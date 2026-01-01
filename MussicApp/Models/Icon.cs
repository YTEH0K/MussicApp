using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MussicApp.Models
{
    public class Icon
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("fileName")]
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string? Url { get; set; }
    }
}

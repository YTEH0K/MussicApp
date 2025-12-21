using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MussicApp.Models
{
    public class AlbumTrack
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string AlbumId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string TrackId { get; set; } = null!;
    }
}

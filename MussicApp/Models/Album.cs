using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MussicApp.Models;

public class Album
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> TrackIds { get; set; } = new();

    public string? CoverFileId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

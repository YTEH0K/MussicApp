using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MussicApp.Models;

public class Track
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string? AlbumId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string FileId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string? CoverFileId { get; set; }

    public TimeSpan Duration { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

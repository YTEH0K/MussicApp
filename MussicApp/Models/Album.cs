using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MussicApp.Models;

public class Album
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public string? CoverFileId { get; set; }

    public ICollection<AlbumTrack> AlbumTracks { get; set; }
        = new List<AlbumTrack>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
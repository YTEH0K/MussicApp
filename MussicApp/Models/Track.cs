using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MussicApp.Models;

public class Track
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    
    public string? Lyrics { get; set; } = string.Empty;
    public Guid ArtistId { get; set; }
    public Artist? Artist { get; set; }

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public Guid? AlbumId { get; set; }
    public Album? Album { get; set; }

    public string FileId { get; set; } = null!;
    public string? CoverFileId { get; set; }

    public TimeSpan Duration { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserLikedTrack> LikedByUsers { get; set; }
        = new List<UserLikedTrack>();
}


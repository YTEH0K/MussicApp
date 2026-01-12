using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MussicApp.Models;

public class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public string? PasswordHash { get; set; }

    public string? GoogleId { get; set; }
    public AuthProvider Provider { get; set; }

    public bool EmailConfirmed { get; set; }
    public string? EmailConfirmCode { get; set; }
    public DateTime? EmailConfirmExpiresAt { get; set; }

    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetExpiresAt { get; set; }

    public string? AvatarFileId { get; set; }

    public ICollection<UserLikedTrack> LikedTracks { get; set; }
        = new List<UserLikedTrack>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


public enum AuthProvider
{
    Local,
    Google
}
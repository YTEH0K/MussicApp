using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MussicApp.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? PasswordHash { get; set; } = string.Empty;

    public string? GoogleId { get; set; }
    public AuthProvider Provider { get; set; }

    public bool EmailConfirmed { get; set; } = false;
    public string? EmailConfirmCode { get; set; }
    public DateTime? EmailConfirmExpiresAt { get; set; }


    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetExpiresAt { get; set; }

    public string? AvatarFileId { get; set; }
    public List<string> LikedTrackIds { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum AuthProvider
{
    Local,
    Google
}
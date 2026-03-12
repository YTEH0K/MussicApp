using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MussicApp.Models.TracksRelated;

namespace MussicApp.Models.UserRelated;

public class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

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

    public ICollection<UserArtistSubscription> ArtistSubscriptions { get; set; }
    = new List<UserArtistSubscription>();

    public ICollection<UserFavoriteGenre> FavoriteGenres { get; set; } = [];
    public UserRole Role { get; set; } = UserRole.User;
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


public enum AuthProvider
{
    Local,
    Google
}

public enum UserRole
{
    User = 0,
    Admin = 1,
    Artist = 2,
    Premium = 3
}
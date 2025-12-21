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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum AuthProvider
{
    Local,
    Google
}
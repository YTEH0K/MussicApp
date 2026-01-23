namespace MussicApp.Models
{
    public class AuthorRequest
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string RequestedUsername { get; set; } = string.Empty;

        public AuthorRequestStatus Status { get; set; }
            = AuthorRequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

public enum AuthorRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
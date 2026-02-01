namespace MussicApp.Models
{
    public class UserListeningHistory
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid TrackId { get; set; }
        public Track Track { get; set; } = null!;

        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

        // Скільки реально слухав (для майбутнього)
        public TimeSpan PlayedDuration { get; set; }
    }
}

public class ListeningHistoryItemDto
{
    public Guid TrackId { get; set; }
    public string Title { get; set; } = null!;
    public string ArtistName { get; set; } = null!;
    public DateTime PlayedAt { get; set; }
    public double PlayedSeconds { get; set; }
}

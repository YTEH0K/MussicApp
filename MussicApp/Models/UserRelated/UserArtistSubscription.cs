namespace MussicApp.Models.UserRelated
{
    public class UserArtistSubscription
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid ArtistId { get; set; }
        public Artist Artist { get; set; } = null!;

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}

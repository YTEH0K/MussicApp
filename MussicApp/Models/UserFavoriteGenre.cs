namespace MussicApp.Models
{
    public class UserFavoriteGenre
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid GenreId { get; set; }
        public Genre Genre { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

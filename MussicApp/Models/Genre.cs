namespace MussicApp.Models
{
    public class Genre
    {
        public Guid Id { get; set; }

        public string Slug { get; set; } = null!; // rock, hip-hop
        public string Name { get; set; } = null!; // Rock
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TrackGenre> TrackGenres { get; set; } = [];
    }

}

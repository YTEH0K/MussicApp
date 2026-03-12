namespace MussicApp.Models.Other
{
    public class Banner
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string FileId { get; set; } = null!;

        public string? LinkUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

namespace MussicApp.Models.Other
{
    public class Advertisement
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ImageFileId { get; set; } = null!;

        public string AudioFileId { get; set; } = null!;

        public string? TargetUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AdvertisementDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        public string AudioUrl { get; set; } = null!;

        public string? TargetUrl { get; set; }
    }
}


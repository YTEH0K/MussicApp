namespace MussicApp.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string? Album { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public string FileType { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}

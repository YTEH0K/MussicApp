namespace MussicApp.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;

        public int? AlbumId { get; set; }
        public Album? Album { get; set; }

        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public string FileType { get; set; } = string.Empty;

        public byte[] CoverData { get; set; } = Array.Empty<byte>();
        public string CoverType { get; set; } = string.Empty;

        public TimeSpan Duration { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AlbumTrack> AlbumTracks { get; set; } = new List<AlbumTrack>();

    }
}

namespace MussicApp.Models
{
    public class Album
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;

        public byte[]? CoverData { get; set; }
        public string? CoverType { get; set; }

        public ICollection<AlbumTrack> AlbumTracks { get; set; } = new List<AlbumTrack>();
    }
}

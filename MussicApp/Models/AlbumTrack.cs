namespace MussicApp.Models
{
    public class AlbumTrack
    {
        public int AlbumId { get; set; }
        public Album Album { get; set; } = null!;

        public int TrackId { get; set; }
        public Track Track { get; set; } = null!;
    }
}

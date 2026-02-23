using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MussicApp.Models.TracksRelated;

namespace MussicApp.Models.AlbumRelated
{
    public class AlbumTrack
    {
        public Guid? AlbumId { get; set; }
        public Album Album { get; set; } = null!;

        public Guid TrackId { get; set; }
        public Track Track { get; set; } = null!;
    }

}

using MussicApp.Models.Other;
using MussicApp.Models.TracksRelated;

namespace MussicApp.Models.UserRelated
{
    public class Artist
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Country Country { get; set; }

        public ICollection<UserArtistSubscription> Subscribers { get; set; }
    = new List<UserArtistSubscription>();

        public ICollection<Track>? Tracks { get; set; }
    }

}

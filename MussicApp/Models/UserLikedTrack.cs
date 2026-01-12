namespace MussicApp.Models;

public class UserLikedTrack
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid TrackId { get; set; }
    public Track Track { get; set; } = null!;
}

using MussicApp.Models.UserRelated;

namespace MussicApp.Models.TracksRelated
{
    public class Comments
    {
        public Guid Id { get; set; }
        public Guid TrackId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Text { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}

namespace MussicApp.Models.TracksRelated
{
    public record CreateCommentDto(
        Guid TrackId,
        string Text
    );
}

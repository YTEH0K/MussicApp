namespace MussicApp.Services
{
    public interface IRadioService
    {

        Task<List<RadioQueueItemDto>> BuildRadioQueueAsync(
            Guid seedTrackId,
            Guid userId,
            int limit
        );

        Task<List<ListeningHistoryItemDto>> GetListeningHistoryAsync(
            Guid userId,
            int limit
        );

        Task<List<RecentlyPlayedDto>> GetRandomRecentlyPlayedAsync(
            Guid userId,
            int sourceLimit,
            int resultLimit
    );
    }
}

public class RadioQueueItemDto
{
    public Guid TrackId { get; set; }
    public string Title { get; set; } = null!;
    public string ArtistName { get; set; } = null!;
    public double Score { get; set; }
    public List<string> Reasons { get; set; } = [];
}

public class RecentlyPlayedDto
{
    public Guid TrackId { get; set; }
    public string Title { get; set; } = null!;
    public string ArtistName { get; set; } = null!;
    public DateTime LastPlayedAt { get; set; }
}

using MussicApp.Models.TracksRelated;

namespace MussicApp.Services.Admin
{
    public interface IAdminTrackService
    {
        Task<List<Track>> GetPendingAsync();
        Task ApproveAsync(Guid trackId);
        Task RejectAsync(Guid trackId);
    }
}

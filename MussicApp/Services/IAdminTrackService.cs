using MussicApp.Models;

namespace MussicApp.Services
{
    public interface IAdminTrackService
    {
        Task<List<Track>> GetPendingAsync();
        Task ApproveAsync(Guid trackId);
        Task RejectAsync(Guid trackId);
    }
}

using MussicApp.Models;

namespace MussicApp.Services
{
    public interface ITrackService
    {
        Task<Track> AddTrackAsync(IFormFile file, IFormFile? cover, string title, string artist, int? albumId);
        Task<IEnumerable<Track>> GetAllAsync();
        Task<Track?> GetByIdAsync(int id);
    }
}

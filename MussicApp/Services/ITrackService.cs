using MussicApp.Models;

namespace MussicApp.Services
{
    public interface ITrackService
    {
        Task<Track> AddTrackAsync(IFormFile file, string title, string artist, string album);
        Task<IEnumerable<Track>> GetAllAsync();
        Task<Track?> GetByIdAsync(int id);
    }
}

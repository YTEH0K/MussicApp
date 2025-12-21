using MussicApp.Models;

public interface ITrackService
{
    Task<Track> AddTrackAsync(
        IFormFile file,
        IFormFile? cover,
        string title,
        string artist,
        string? albumId);

    Task<IEnumerable<Track>> GetAllAsync();
    Task<Track?> GetByIdAsync(string id);
}

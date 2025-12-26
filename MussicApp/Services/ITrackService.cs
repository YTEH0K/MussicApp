using MussicApp.Models;

public interface ITrackService
{
    Task<Track> AddTrackAsync(
        IFormFile file,
        IFormFile cover,
        string title,
        string artist,
        string? albumId,
        string ownerId);

    Task<IEnumerable<Track>> GetAllAsync();
    Task<Track?> GetByIdAsync(string id);
    Task DeleteAsync(Track track);
    Task<IEnumerable<Track>> GetByOwnerIdAsync(string ownerId);
}

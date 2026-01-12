using MongoDB.Driver;
using MussicApp.Models;

public interface ITrackService
{
    Task<Track> CreateAsync(
       IFormFile file,
       IFormFile cover,
       string title,
       Guid artistId,
       Guid? albumId,
       Guid ownerId);


    Task<IEnumerable<Track>> GetAllAsync();
    Task<Track?> GetByIdAsync(Guid id);
    Task DeleteAsync(Track track);
    Task<IEnumerable<Track>> GetByOwnerIdAsync(Guid ownerId);

    Task<IEnumerable<Artist>> GetAllArtistsAsync();
}
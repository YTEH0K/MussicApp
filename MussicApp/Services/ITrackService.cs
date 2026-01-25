using MongoDB.Driver;
using MussicApp.Models;

public interface ITrackService
{
    Task<Track> CreateAsync(
       IFormFile file,
       IFormFile cover,
       string title,
       string lyrics,
       Guid artistId,
       Guid? albumId,
       Guid ownerId,
       IEnumerable<Guid> genreIds);

    Task<IEnumerable<Track>> GetAllAsync();
    Task<Track?> GetByIdAsync(Guid id);
    Task DeleteAsync(Track track);
    Task<IEnumerable<Track>> GetByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<Artist>> GetAllArtistsAsync();
    Task<Track> UpdateAsync(Track track);
    Task<IEnumerable<Track>> GetByGenreSlugAsync(string slug);
    //Task<Track?> GetLyricById (Guid id);
}
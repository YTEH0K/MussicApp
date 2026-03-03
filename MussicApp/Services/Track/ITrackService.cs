using MongoDB.Driver;
using MussicApp.Models.TracksRelated;
using MussicApp.Models.UserRelated;

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
    Task AddListeningHistoryAsync(Guid userId, Guid trackId, TimeSpan playedDuration);
    Task<IEnumerable<UserListeningHistory>> GetListeningHistoryAsync(Guid userId, int limit = 50);
    Task SetUserFavoriteGenresAsync(Guid userId, IEnumerable<Guid> genreIds);
    Task<IEnumerable<Genre>> GetAllGenresAsync();
    Task<List<Track>> SearchByNameAsync(string query);
    Task<List<Track>> SearchByArtistAsync(string artistName);
    Task<List<Track>> SearchByGenreNameAsync(string genreName);
    Task<List<Track>> GetRecentRandomAsync(Guid userId);

    //Task<Track?> GetLyricById (Guid id);
}
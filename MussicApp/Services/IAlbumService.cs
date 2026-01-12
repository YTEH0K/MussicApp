using Microsoft.AspNetCore.Http;
using MussicApp.Models;

public interface IAlbumService
{
    Task<Album> CreateAsync(string title, string artist,Guid OwnerId, IFormFile? cover = null);
    Task<bool> AddCoverAsync(Guid albumId, IFormFile cover);
    Task<bool> AddTrackAsync(Guid albumId, Guid trackId);
    Task<Album?> GetByIdAsync(Guid albumId);
    Task<IEnumerable<Album>> GetByOwnerAsync(Guid ownerId);

    Task<(byte[]? Data, string? ContentType)> GetCoverAsync(Guid albumId);
    Task<IEnumerable<Track>> GetTracksAsync(Guid albumId);
    Task<IEnumerable<Album>> GetAllAsync();
}

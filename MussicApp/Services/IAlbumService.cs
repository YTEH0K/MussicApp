using Microsoft.AspNetCore.Http;
using MussicApp.Models;

public interface IAlbumService
{
    Task<Album> CreateAsync(string title, string artist, IFormFile? cover = null);
    Task<bool> AddCoverAsync(string albumId, IFormFile cover);
    Task<bool> AddTrackAsync(string albumId, string trackId);
    Task<Album?> GetByIdAsync(string albumId);
    Task<(byte[]? Data, string? ContentType)> GetCoverAsync(string albumId);
    Task<IEnumerable<Track>> GetTracksAsync(string albumId);
    Task<IEnumerable<Album>> GetAllAsync();
}

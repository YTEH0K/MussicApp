using Microsoft.EntityFrameworkCore;
using MussicApp.Models;
using MussicApp.Services;


public class TrackService : ITrackService
{
    private readonly AppDbContext _db;


    public TrackService(AppDbContext db)
    {
        _db = db;
    }


    public async Task<Track> AddTrackAsync(IFormFile file, IFormFile? cover, string title, string artist, int? albumId)
    {
        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        byte[]? coverBytes = null;
        string? coverType = null;

        if (cover != null)
        {
            using var ms2 = new MemoryStream();
            await cover.CopyToAsync(ms2);
            coverBytes = ms2.ToArray();
            coverType = cover.ContentType;
        }

        var track = new Track
        {
            Title = title,
            Artist = artist,
            AlbumId = albumId,
            FileData = fileBytes,
            FileType = file.ContentType,
            CoverData = coverBytes,
            CoverType = coverType
        };

        _db.Tracks.Add(track);
        await _db.SaveChangesAsync();
        return track;
    }


    public async Task<IEnumerable<Track>> GetAllAsync()
    {
        return await _db.Tracks.OrderBy(t => t.Title).ToListAsync();
    }


    public async Task<Track?> GetByIdAsync(int id)
    {
        return await _db.Tracks.FindAsync(id);
    }
}
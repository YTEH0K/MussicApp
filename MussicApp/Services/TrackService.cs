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


    public async Task<Track> AddTrackAsync(IFormFile file, string title, string artist, string album)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);


        var track = new Track
        {
            Title = title,
            Artist = artist,
            Album = album,
            FileData = ms.ToArray(),
            FileType = file.ContentType,
            Duration = TimeSpan.Zero
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
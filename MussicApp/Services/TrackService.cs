using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MussicApp.Models;
using MussicApp.Data;
using MongoDB.Driver.GridFS;
using Microsoft.EntityFrameworkCore;

namespace MussicApp.Services;

public class TrackService : ITrackService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _gridFS;

    public TrackService(
        AppDbContext db,
        IMongoClient mongo,
        IFileStorageService gridFS,
        IOptions<MongoDbSettings> options)
    {
        _db = db;
        _gridFS = gridFS;
    }

    public async Task<Track> CreateAsync(
    IFormFile audio,
    IFormFile cover,
    string title,
    Guid artistId,
    Guid? albumId,
    Guid ownerId)
    {
        if (audio == null || audio.Length == 0)
            throw new ArgumentException("Audio file is required");

        if (cover == null || cover.Length == 0)
            throw new ArgumentException("Cover image is required");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required");

        // 🔒 Перевірка артиста (FK safety)
        var artistExists = await _db.Artists.AnyAsync(a => a.Id == artistId);
        if (!artistExists)
            throw new InvalidOperationException(
                $"Artist with id {artistId} does not exist"
            );

        ObjectId audioId = ObjectId.Empty;
        ObjectId coverId = ObjectId.Empty;

        try
        {
            // 🎵 upload audio
            await using (var audioStream = audio.OpenReadStream())
            {
                audioId = await _gridFS.UploadAsync(
                    audioStream,
                    audio.FileName,
                    audio.ContentType
                );
            }

            // 🖼 upload cover
            await using (var coverStream = cover.OpenReadStream())
            {
                coverId = await _gridFS.UploadAsync(
                    coverStream,
                    cover.FileName,
                    cover.ContentType
                );
            }

            var track = new Track
            {
                Id = Guid.NewGuid(),
                Title = title,
                ArtistId = artistId,
                AlbumId = albumId,
                OwnerId = ownerId,
                FileId = audioId.ToString(),
                CoverFileId = coverId.ToString(),
                UploadedAt = DateTime.UtcNow
            };

            _db.Tracks.Add(track);
            await _db.SaveChangesAsync();

            return track;
        }
        catch
        {
            if (audioId != ObjectId.Empty)
                await _gridFS.DeleteAsync(audioId);

            if (coverId != ObjectId.Empty)
                await _gridFS.DeleteAsync(coverId);

            throw;
        }
    }





    public async Task DeleteAsync(Track track)
    {
        await _gridFS.DeleteAsync(ObjectId.Parse(track.FileId));

        if (!string.IsNullOrEmpty(track.CoverFileId))
            await _gridFS.DeleteAsync(ObjectId.Parse(track.CoverFileId));

        _db.Tracks.Remove(track);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Track>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _db.Tracks.Where(t => t.OwnerId == ownerId).ToListAsync();
    }

    public async Task<IEnumerable<Track>> GetAllAsync()
    {
        return await _db.Tracks.ToListAsync();
    }

    public async Task<Track?> GetByIdAsync(Guid id)
    {
        return await _db.Tracks
           .Include(t => t.Album)
           .FirstOrDefaultAsync(t => t.Id == id);
    }


    public async Task<IEnumerable<Artist>> GetAllArtistsAsync()
    {
        return await _db.Artists.ToListAsync();
    }

    public async Task<Artist?> GetArtistByIdAsync(Guid id)
    {
        return await _db.Artists.FindAsync(id);
    }

}

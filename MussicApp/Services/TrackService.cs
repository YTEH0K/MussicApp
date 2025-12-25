using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MussicApp.Models;

namespace MussicApp.Services;

public class TrackService : ITrackService
{
    private readonly IMongoCollection<Track> _tracks;
    private readonly IFileStorageService _files;

    public TrackService(
        IMongoClient client,
        IOptions<MongoDbSettings> options,
        IFileStorageService files)
    {
        var db = client.GetDatabase(options.Value.DatabaseName);
        _tracks = db.GetCollection<Track>("tracks");
        _files = files;
    }

    public async Task<Track> AddTrackAsync(
        IFormFile file,
        IFormFile? cover,
        string title,
        string artist,
        string? albumId,
        string ownerId)
    {
        ObjectId audioFileId;
        using (var stream = file.OpenReadStream())
        {
            audioFileId = await _files.UploadAsync(
                stream,
                file.FileName,
                file.ContentType);
        }

        ObjectId? coverFileId = null;
        if (cover != null)
        {
            using var coverStream = cover.OpenReadStream();
            coverFileId = await _files.UploadAsync(
                coverStream,
                cover.FileName,
                cover.ContentType);
        }

        var track = new Track
        {
            Title = title,
            Artist = artist,
            AlbumId = albumId,
            OwnerId = ownerId,
            OwnerUsername = artist,
            FileId = audioFileId.ToString(),
            CoverFileId = coverFileId?.ToString()
        };

        await _tracks.InsertOneAsync(track);
        return track;
    }

    public async Task DeleteAsync(Track track)
    {
        await _files.DeleteAsync(ObjectId.Parse(track.FileId));

        if (!string.IsNullOrEmpty(track.CoverFileId))
            await _files.DeleteAsync(ObjectId.Parse(track.CoverFileId));

        await _tracks.DeleteOneAsync(t => t.Id == track.Id);
    }

    public async Task<IEnumerable<Track>> GetByOwnerIdAsync(string ownerId)
    {
        return await _tracks.Find(t => t.OwnerId == ownerId).ToListAsync();
    }

    public async Task<IEnumerable<Track>> GetAllAsync()
    {
        return await _tracks.Find(_ => true).ToListAsync();
    }

    public async Task<Track?> GetByIdAsync(string id)
    {
        return await _tracks.Find(t => t.Id == id).FirstOrDefaultAsync();
    }
}

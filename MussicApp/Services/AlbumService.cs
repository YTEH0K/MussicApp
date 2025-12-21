using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MussicApp.Models;
using MussicApp.Services;

public class AlbumService : IAlbumService
{
    private readonly IMongoCollection<Album> _albums;
    private readonly IMongoCollection<Track> _tracks;
    private readonly GridFSBucket _gridFS;

    public AlbumService(IMongoClient client, IOptions<MongoDbSettings> options)
    {
        var db = client.GetDatabase(options.Value.DatabaseName);
        _albums = db.GetCollection<Album>("albums");
        _tracks = db.GetCollection<Track>("tracks");
        _gridFS = new GridFSBucket(db);
    }

    public async Task<Album> CreateAsync(string title, string artist, IFormFile? cover = null)
    {
        string? coverFileId = null;

        if (cover != null)
        {
            using var ms = new MemoryStream();
            await cover.CopyToAsync(ms);
            coverFileId = (await _gridFS.UploadFromBytesAsync(cover.FileName, ms.ToArray())).ToString();
        }

        var album = new Album
        {
            Title = title,
            Artist = artist,
            CoverFileId = coverFileId
        };

        await _albums.InsertOneAsync(album);
        return album;
    }

    public async Task<bool> AddCoverAsync(string albumId, IFormFile cover)
    {
        var album = await _albums.Find(a => a.Id == albumId).FirstOrDefaultAsync();
        if (album == null) return false;

        using var ms = new MemoryStream();
        await cover.CopyToAsync(ms);
        var coverFileId = (await _gridFS.UploadFromBytesAsync(cover.FileName, ms.ToArray())).ToString();

        album.CoverFileId = coverFileId;
        await _albums.ReplaceOneAsync(a => a.Id == albumId, album);

        return true;
    }
    public async Task<bool> AddTrackAsync(string albumId, string trackId)
    {
        var album = await _albums.Find(a => a.Id == albumId).FirstOrDefaultAsync();
        var track = await _tracks.Find(t => t.Id == trackId).FirstOrDefaultAsync();
        if (album == null || track == null) return false;

        if (!album.TrackIds.Contains(trackId))
        {
            album.TrackIds.Add(trackId);
            await _albums.ReplaceOneAsync(a => a.Id == albumId, album);
        }

        return true;
    }

    public async Task<Album?> GetByIdAsync(string albumId)
    {
        return await _albums.Find(a => a.Id == albumId).FirstOrDefaultAsync();
    }

    public async Task<(byte[]? Data, string? ContentType)> GetCoverAsync(string albumId)
    {
        var album = await _albums.Find(a => a.Id == albumId).FirstOrDefaultAsync();
        if (album == null || string.IsNullOrEmpty(album.CoverFileId)) return (null, null);

        var objectId = MongoDB.Bson.ObjectId.Parse(album.CoverFileId);
        var data = await _gridFS.DownloadAsBytesAsync(objectId);

        var fileInfo = await _gridFS.Find(Builders<GridFSFileInfo>.Filter.Eq("_id", objectId)).FirstOrDefaultAsync();
        var contentType = fileInfo?.Metadata?["ContentType"]?.AsString ?? "application/octet-stream";

        return (data, contentType);
    }

    public async Task<IEnumerable<Track>> GetTracksAsync(string albumId)
    {
        var album = await _albums.Find(a => a.Id == albumId).FirstOrDefaultAsync();
        if (album == null) return Array.Empty<Track>();

        var filter = Builders<Track>.Filter.In(t => t.Id, album.TrackIds);
        return await _tracks.Find(filter).ToListAsync();
    }
    public async Task<IEnumerable<Album>> GetAllAsync()
    {
        return await _albums.Find(_ => true).ToListAsync();
    }
}

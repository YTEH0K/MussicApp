using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MussicApp.Data;
using MussicApp.Models;
using MussicApp.Services;

public class AlbumService : IAlbumService
{
    //private readonly IMongoCollection<Album> _albums;
    //private readonly IMongoCollection<Track> _tracks;

    private readonly AppDbContext _db;
    private readonly GridFSBucket _gridFS;

    public AlbumService(AppDbContext db, IMongoClient client, IOptions<MongoDbSettings> options)
    {
        _db = db; 
        var mongoDb = client.GetDatabase(options.Value.DatabaseName);
        _gridFS = new GridFSBucket(mongoDb);
    }

    public async Task<Album> CreateAsync(
        string title,
        string artist,
        Guid ownerId,
        IFormFile? cover)
    {
        string? coverFileId = null;

        if (cover != null)
        {
            using var ms = new MemoryStream();
            await cover.CopyToAsync(ms);

            coverFileId = (
                await _gridFS.UploadFromBytesAsync(
                    cover.FileName,
                    ms.ToArray(),
                    new GridFSUploadOptions
                    {
                        Metadata = new MongoDB.Bson.BsonDocument
                        {
                            { "ContentType", cover.ContentType }
                        }
                    }
                )
            ).ToString();
        }

        var album = new Album
        {
            Id = Guid.NewGuid(),
            Title = title,
            Artist = artist,
            OwnerId = ownerId,
            CoverFileId = coverFileId
        };

        _db.Albums.Add(album);
        await _db.SaveChangesAsync();

        return album;
    }


    public async Task<bool> AddCoverAsync(Guid albumId, IFormFile cover)
    {
        var album = await _db.Albums.FindAsync(albumId);
        if (album == null) return false;

        using var ms = new MemoryStream();
        await cover.CopyToAsync(ms);

        album.CoverFileId = (
            await _gridFS.UploadFromBytesAsync(
                cover.FileName,
                ms.ToArray(),
                new GridFSUploadOptions
                {
                    Metadata = new MongoDB.Bson.BsonDocument
                    {
                        { "ContentType", cover.ContentType }
                    }
                }
            )
        ).ToString();

        await _db.SaveChangesAsync();
        return true;
    }
    public async Task<bool> AddTrackAsync(Guid albumId, Guid trackId)
    {
        var exists = await _db.AlbumTracks
            .AnyAsync(at => at.AlbumId == albumId && at.TrackId == trackId);

        if (exists) return true;

        var album = await _db.Albums.FindAsync(albumId);
        var track = await _db.Tracks.FindAsync(trackId);

        if (album == null || track == null) return false;

        _db.AlbumTracks.Add(new AlbumTrack
        {
            AlbumId = albumId,
            TrackId = trackId
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Album?> GetByIdAsync(Guid albumId)
    {
        return await _db.Albums
            .Include(a => a.AlbumTracks)
            .FirstOrDefaultAsync(a => a.Id == albumId);
    }

    public async Task<IEnumerable<Album>> GetByOwnerAsync(Guid ownerId)
    {
        return await _db.Albums
            .Where(a => a.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Album>> GetAllAsync()
    {
        return await _db.Albums.ToListAsync();
    }

    public async Task<IEnumerable<Track>> GetTracksAsync (Guid albumId)
    {
        return await _db.AlbumTracks
            .Where(at => at.AlbumId == albumId)
            .Select(at => at.Track)
            .ToListAsync();
    }


    public async Task<(byte[]? Data, string? ContentType)> GetCoverAsync(Guid albumId)
    {
        var album = await _db.Albums.FindAsync(albumId);
        if (album?.CoverFileId == null) return (null, null);

        var ObjectId = MongoDB.Bson.ObjectId.Parse(album.CoverFileId);
        var data = await _gridFS.DownloadAsBytesAsync(ObjectId);

        var info = await _gridFS
            .Find(Builders<GridFSFileInfo>.Filter.Eq("_id", ObjectId))
            .FirstOrDefaultAsync();

        var contentType =
            info?.Metadata?["ContentType"]?.AsString
            ?? "application/octet-stream";

        return (data, contentType);
    }

    //public async Task<IEnumerable<Track>> GetTracksAsync(string albumId)
    //{
    //    var album = await _albums.Find(a => a.Id == albumId).FirstOrDefaultAsync();
    //    if (album == null) return Array.Empty<Track>();

    //    var filter = Builders<Track>.Filter.In(t => t.Id, album.TrackIds);
    //    return await _tracks.Find(filter).ToListAsync();
    //}
    //public async Task<IEnumerable<Album>> GetAllAsync()
    //{
    //    return await _albums.Find(_ => true).ToListAsync();
    //}
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MussicApp.Data;
using MussicApp.Models;
using MussicApp.Models.Other;
using MussicApp.Services.Banner;
using MussicApp.Services.Other;

public class BannerService : IBannerService
{
    private readonly AppDbContext _db;
    private readonly GridFSBucket _gridFs;

    public BannerService(
        AppDbContext db,
        IMongoClient mongo,
        IOptions<MongoDbSettings> options)
    {
        _db = db;

        _gridFs = new GridFSBucket(
            mongo.GetDatabase(options.Value.DatabaseName));
    }

    public async Task<Banner> UploadAsync(
        IFormFile file,
        string title,
        string? link)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var fileId = await _gridFs.UploadFromBytesAsync(
            file.FileName,
            ms.ToArray(),
            new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    { "ContentType", file.ContentType }
                }
            });

        var banner = new Banner
        {
            Id = Guid.NewGuid(),
            Title = title,
            FileId = fileId.ToString(),
            LinkUrl = link
        };

        _db.Banners.Add(banner);
        await _db.SaveChangesAsync();

        return banner;
    }

    public async Task<IEnumerable<Banner>> GetAllAsync()
    {
        return await _db.Banners
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<(byte[] Data, string ContentType)?> GetImageAsync(Guid bannerId)
    {
        var banner = await _db.Banners.FindAsync(bannerId);
        if (banner == null)
            return null;

        var objectId = ObjectId.Parse(banner.FileId);

        var data = await _gridFs.DownloadAsBytesAsync(objectId);

        var info = await _gridFs
            .Find(Builders<GridFSFileInfo>.Filter.Eq("_id", objectId))
            .FirstOrDefaultAsync();

        var contentType =
            info?.Metadata?["ContentType"]?.AsString
            ?? "application/octet-stream";

        return (data, contentType);
    }

    public async Task DeleteAsync(Guid bannerId)
    {
        var banner = await _db.Banners.FindAsync(bannerId);

        if (banner == null)
            return;

        var objectId = ObjectId.Parse(banner.FileId);

        await _gridFs.DeleteAsync(objectId);

        _db.Banners.Remove(banner);

        await _db.SaveChangesAsync();
    }
}
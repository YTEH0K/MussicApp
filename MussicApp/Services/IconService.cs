using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MussicApp.Models;
using MussicApp.Services;

public class IconService : IIconService
{
    private readonly IMongoCollection<Icon> _icons;
    private readonly IFileStorageService _files;

    public IconService(IMongoClient client, IOptions<MongoDbSettings> options, IFileStorageService files)
    {
        var db = client.GetDatabase(options.Value.DatabaseName);
        _icons = db.GetCollection<Icon>("icons");
        _files = files;

        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexKeys = Builders<Icon>.IndexKeys.Ascending(i => i.FileName);
        _icons.Indexes.CreateOne(new CreateIndexModel<Icon>(indexKeys, indexOptions));
    }

    public async Task<Icon> UploadIconAsync(IFormFile file)
    {
        var exists = await _icons.Find(i => i.FileName == file.FileName).AnyAsync();
        if (exists)
            throw new Exception("Icon with this FileName already exists");

        ObjectId fileId;
        using var stream = file.OpenReadStream();
        fileId = await _files.UploadAsync(stream, file.FileName, file.ContentType);

        var icon = new Icon
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Url = $"/files/{fileId}"
        };

        await _icons.InsertOneAsync(icon);
        return icon;
    }

    public async Task<IEnumerable<Icon>> GetAllIconsAsync()
    {
        return await _icons.Find(_ => true).ToListAsync();
    }

    public async Task<Icon?> GetByFileNameAsync(string fileName)
    {
        return await _icons.Find(i => i.FileName == fileName).FirstOrDefaultAsync();
    }

    public async Task DeleteIconAsync(string fileName)
    {
        var icon = await GetByFileNameAsync(fileName);
        if (icon == null) return;

        if (!string.IsNullOrEmpty(icon.Url))
        {
            var parts = icon.Url.Split('/');
            if (parts.Length > 1 && ObjectId.TryParse(parts[1], out var fileId))
                await _files.DeleteAsync(fileId);
        }

        await _icons.DeleteOneAsync(i => i.FileName == fileName);
    }
}

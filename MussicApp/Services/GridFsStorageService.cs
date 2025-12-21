using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Microsoft.Extensions.Options;

namespace MussicApp.Services;

public class GridFsStorageService : IFileStorageService
{
    private readonly GridFSBucket _bucket;

    public GridFsStorageService(
        IMongoClient client,
        IOptions<MongoDbSettings> options)
    {
        var db = client.GetDatabase(options.Value.DatabaseName);

        _bucket = new GridFSBucket(db, new GridFSBucketOptions
        {
            BucketName = "media"
        });
    }

    public async Task<ObjectId> UploadAsync(
        Stream stream,
        string fileName,
        string contentType)
    {
        var options = new GridFSUploadOptions
        {
            Metadata = new BsonDocument
            {
                { "contentType", contentType }
            }
        };

        return await _bucket.UploadFromStreamAsync(
            fileName,
            stream,
            options);
    }

    public async Task<(Stream Stream, string ContentType)> DownloadAsync(ObjectId fileId)
    {
        var fileInfo = await _bucket.Find(Builders<GridFSFileInfo>.Filter.Eq("_id", fileId))
            .FirstOrDefaultAsync();

        if (fileInfo == null)
            throw new FileNotFoundException();

        var stream = new MemoryStream();
        await _bucket.DownloadToStreamAsync(fileId, stream);
        stream.Position = 0;

        var contentType = fileInfo.Metadata?["contentType"]?.AsString
                          ?? "application/octet-stream";

        return (stream, contentType);
    }

    public async Task DeleteAsync(ObjectId fileId)
    {
        await _bucket.DeleteAsync(fileId);
    }
}

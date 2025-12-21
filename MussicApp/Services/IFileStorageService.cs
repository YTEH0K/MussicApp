using MongoDB.Bson;

public interface IFileStorageService
{
    Task<ObjectId> UploadAsync(
        Stream stream,
        string fileName,
        string contentType);

    Task<(Stream Stream, string ContentType)> DownloadAsync(ObjectId fileId);

    Task DeleteAsync(ObjectId fileId);
}

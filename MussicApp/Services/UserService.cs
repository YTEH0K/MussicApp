using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MussicApp.Models;
using MongoDB.Driver.GridFS;
namespace MussicApp.Services;

public class UserService : IUserService
{
    private readonly IMongoCollection<User> _users;
    private readonly GridFSBucket _gridFS;

    public UserService(
        IMongoClient client,
        IOptions<MongoDbSettings> options)
    {
        var db = client.GetDatabase(options.Value.DatabaseName);
        _users = db.GetCollection<User>("users");

        _gridFS = new GridFSBucket(db);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _users
            .Find(u => u.Username == username)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId)
    {
        return await _users
            .Find(u => u.GoogleId == googleId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _users
            .Find(u => u.Email == email)
            .AnyAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _users
            .Find(u => u.Email == email)
            .AnyAsync();
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _users
             .Find(u => u.Username == username)
             .AnyAsync();
    }

    public async Task CreateAsync(User user)
    {
        await _users.InsertOneAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
    }

    public async Task AddLikeAsync(string userId, string trackId)
    {
        var update = Builders<User>.Update.AddToSet(u => u.LikedTrackIds, trackId);
        await _users.UpdateOneAsync(u => u.Id == userId, update);
    }

    public async Task RemoveLikeAsync(string userId, string trackId)
    {
        var update = Builders<User>.Update.Pull(u => u.LikedTrackIds, trackId);
        await _users.UpdateOneAsync(u => u.Id == userId, update);
    }

    public async Task<IEnumerable<string>> GetLikedTrackIdsAsync(string userId)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return user?.LikedTrackIds ?? Enumerable.Empty<string>();
    }

    public async Task SetAvatarAsync(string userId, IFormFile avatar)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null) throw new Exception("User not found");

        using var ms = new MemoryStream();
        await avatar.CopyToAsync(ms);

        var fileId = await _gridFS.UploadFromBytesAsync(
            avatar.FileName,
            ms.ToArray(),
            new GridFSUploadOptions
            {
                Metadata = new MongoDB.Bson.BsonDocument
                {
                    { "ContentType", avatar.ContentType }
                }
            });

        user.AvatarFileId = fileId.ToString();
        await UpdateAsync(user);
    }

    public async Task<(byte[] Data, string ContentType)?> GetAvatarAsync(string userId)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null || user.AvatarFileId == null)
            return null;

        var objectId = MongoDB.Bson.ObjectId.Parse(user.AvatarFileId);

        var bytes = await _gridFS.DownloadAsBytesAsync(objectId);
        var fileInfo = await _gridFS
            .Find(Builders<GridFSFileInfo>.Filter.Eq("_id", objectId))
            .FirstOrDefaultAsync();

        var contentType =
            fileInfo?.Metadata?["ContentType"]?.AsString
            ?? "application/octet-stream";

        return (bytes, contentType);
    }

}

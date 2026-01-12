using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MussicApp.Models;
using MongoDB.Driver.GridFS;
using MussicApp.Data;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
namespace MussicApp.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly GridFSBucket _gridFs;

    public UserService(
        AppDbContext db,
        IMongoClient mongo,
        IOptions<MongoDbSettings> options)
    {
        _db = db;
        _gridFs = new GridFSBucket(
            mongo.GetDatabase(options.Value.DatabaseName));
    }

    public Task<User?> GetByIdAsync(Guid id) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(string email) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByUsernameAsync(string username) =>
        _db.Users.FirstOrDefaultAsync(u => u.Username == username);

    public Task<User?> GetByGoogleIdAsync(string googleId) =>
        _db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

    public Task<bool> ExistsByEmailAsync(string email) =>
        _db.Users.AnyAsync(u => u.Email == email);

    public Task<bool> ExistsByUsernameAsync(string username) =>
        _db.Users.AnyAsync(u => u.Username == username);


    public async Task CreateAsync(User user)
    {
        user.Id = Guid.NewGuid();
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }


    public async Task AddLikeAsync(Guid userId, Guid trackId)
    {
        var exists = await _db.UserLikedTracks
            .AnyAsync(x => x.UserId == userId && x.TrackId == trackId);

        if (!exists)
        {
            _db.UserLikedTracks.Add(new UserLikedTrack
            {
                UserId = userId,
                TrackId = trackId
            });

            await _db.SaveChangesAsync();
        }
    }

    public async Task RemoveLikeAsync(Guid userId, Guid trackId)
    {
        var like = await _db.UserLikedTracks
            .FirstOrDefaultAsync(x =>
                x.UserId == userId && x.TrackId == trackId);

        if (like != null)
        {
            _db.UserLikedTracks.Remove(like);
            await _db.SaveChangesAsync();
        }
    }


    public async Task<IEnumerable<Guid>> GetLikedTrackIdsAsync(Guid userId)
    {
        return await _db.UserLikedTracks
            .Where(x => x.UserId == userId)
            .Select(x => x.TrackId)
            .ToListAsync();
    }


    public async Task SetAvatarAsync(Guid userId, IFormFile avatar)
    {
        var user = await GetByIdAsync(userId)
            ?? throw new Exception("User not found");

        using var ms = new MemoryStream();
        await avatar.CopyToAsync(ms);

        var fileId = await _gridFs.UploadFromBytesAsync(
            avatar.FileName,
            ms.ToArray(),
            new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    { "ContentType", avatar.ContentType }
                }
            });

        user.AvatarFileId = fileId.ToString();
        await UpdateAsync(user);
    }

    public async Task<(byte[] Data, string ContentType)?> GetAvatarAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user?.AvatarFileId == null)
            return null;

        var objectId = ObjectId.Parse(user.AvatarFileId);
        var data = await _gridFs.DownloadAsBytesAsync(objectId);

        var info = await _gridFs
            .Find(Builders<GridFSFileInfo>.Filter.Eq("_id", objectId))
            .FirstOrDefaultAsync();

        var contentType =
            info?.Metadata?["ContentType"]?.AsString
            ?? "application/octet-stream";

        return (data, contentType);
    }

}

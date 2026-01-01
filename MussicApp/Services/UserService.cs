using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MussicApp.Models;

namespace MussicApp.Services;

public class UserService : IUserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(
        IMongoClient client,
        IOptions<MongoDbSettings> options)
    {
        var db = client.GetDatabase(options.Value.DatabaseName);
        _users = db.GetCollection<User>("users");
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

    public async Task ChangeAvatarAsync(string userId, string avatarUrl)
    {
        var update = Builders<User>.Update.Set(u => u.AvatarUrl, avatarUrl);
        await _users.UpdateOneAsync(u => u.Id == userId, update);
    }

}

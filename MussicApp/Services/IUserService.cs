using MussicApp.Models;

namespace MussicApp.Services
{
    public interface IUserService
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> ExistsAsync(string username);


        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);

        Task UpdateAsync(User user);


        Task<User?> GetByGoogleIdAsync(string googleId);
        Task CreateAsync(User user);



        Task AddLikeAsync(string userId, string trackId);
        Task RemoveLikeAsync(string userId, string trackId);
        Task<IEnumerable<string>> GetLikedTrackIdsAsync(string userId);
        Task ChangeAvatarAsync(string userId, string avatarUrl);
    }
}

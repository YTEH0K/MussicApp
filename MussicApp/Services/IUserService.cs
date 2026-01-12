using MussicApp.Models;

namespace MussicApp.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByGoogleIdAsync(string googleId);
        //Task<bool> ExistsAsync(string username);


        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task CreateAsync(User user);
        Task UpdateAsync(User user);




        Task AddLikeAsync(Guid userId, Guid trackId);
        Task RemoveLikeAsync(Guid userId, Guid trackId);
        Task<IEnumerable<Guid>> GetLikedTrackIdsAsync(Guid userId);

        Task SetAvatarAsync(Guid userId, IFormFile avatar);
        Task<(byte[] Data, string ContentType)?> GetAvatarAsync(Guid userId);
    }
}

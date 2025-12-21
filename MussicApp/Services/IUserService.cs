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



        Task<User?> GetByGoogleIdAsync(string googleId);
        Task CreateAsync(User user);
    }
}

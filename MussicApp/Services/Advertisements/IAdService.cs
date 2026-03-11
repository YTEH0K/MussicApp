using MussicApp.Models.Other;

namespace MussicApp.Services.Advertisements
{
    public interface IAdService
    {
        Task<Advertisement> UploadAsync(
            IFormFile image,
            IFormFile audio,
            string title,
            string? targetUrl);

        Task<IEnumerable<Advertisement>> GetAllAsync();

        Task<Advertisement?> GetByIdAsync(Guid id);

        Task<Advertisement?> GetRandomAsync();

        Task DisableAsync(Guid id);
    }
}

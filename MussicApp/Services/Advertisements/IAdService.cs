using MussicApp.Models.Other;

namespace MussicApp.Services.Advertisements
{
    public interface IAdService
    {
        Task<Advertisement> UploadAsync(
            IFormFile image,
            string title,
            string? targetUrl);

        Task<Advertisement?> GetRandomAsync();

        Task<Advertisement?> GetByIdAsync(Guid id);
    }
}

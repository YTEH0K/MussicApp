namespace MussicApp.Services.Banner
{
    using MussicApp.Models.Other;

    public interface IBannerService
    {
        Task<Banner> UploadAsync(IFormFile file, string title, string? link);

        Task<IEnumerable<Banner>> GetAllAsync();

        Task<(byte[] Data, string ContentType)?> GetImageAsync(Guid bannerId);

        Task DeleteAsync(Guid bannerId);
    }
}

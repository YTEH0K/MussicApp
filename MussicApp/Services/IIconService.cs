using MussicApp.Models;

namespace MussicApp.Services
{
    public interface IIconService
    {
        Task<Icon> UploadIconAsync(IFormFile file);          // загрузка новой иконки
        Task<IEnumerable<Icon>> GetAllIconsAsync();          // получить все иконки
        Task<Icon?> GetByFileNameAsync(string fileName);                // получить конкретную иконку
        Task DeleteIconAsync(string id);
    }
}

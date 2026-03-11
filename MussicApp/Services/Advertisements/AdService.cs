using MussicApp.Data;
using MussicApp.Models.Other;
using MongoDB.Bson;
using Microsoft.EntityFrameworkCore;
using MussicApp.Data;

namespace MussicApp.Services.Advertisements
{

    public class AdService : IAdService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _files;

        public AdService(
            AppDbContext context,
            IFileStorageService files)
        {
            _context = context;
            _files = files;
        }

        public async Task<Advertisement> UploadAsync(
            IFormFile image,
            string title,
            string? targetUrl)
        {
            ObjectId fileId;

            await using (var stream = image.OpenReadStream())
            {
                fileId = await _files.UploadAsync(
                    stream,
                    image.FileName,
                    image.ContentType);
            }

            var ad = new Advertisement
            {
                Id = Guid.NewGuid(),
                Title = title,
                ImageFileId = fileId.ToString(),
                TargetUrl = targetUrl
            };

            _context.Advertisements.Add(ad);
            await _context.SaveChangesAsync();

            return ad;
        }

        public async Task<Advertisement?> GetRandomAsync()
        {
            return await _context.Advertisements
                .Where(a => a.IsActive)
                .OrderBy(x => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        public async Task<Advertisement?> GetByIdAsync(Guid id)
        {
            return await _context.Advertisements
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MussicApp.Data;
using MussicApp.Models.Other;

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
            IFormFile audio,
            string title,
            string? targetUrl)
        {
            ObjectId imageId;
            ObjectId audioId;

            await using (var stream = image.OpenReadStream())
            {
                imageId = await _files.UploadAsync(
                    stream,
                    image.FileName,
                    image.ContentType);
            }

            await using (var stream = audio.OpenReadStream())
            {
                audioId = await _files.UploadAsync(
                    stream,
                    audio.FileName,
                    audio.ContentType);
            }

            var ad = new Advertisement
            {
                Id = Guid.NewGuid(),
                Title = title,
                ImageFileId = imageId.ToString(),
                AudioFileId = audioId.ToString(),
                TargetUrl = targetUrl
            };

            _context.Advertisements.Add(ad);

            await _context.SaveChangesAsync();

            return ad;
        }

        public async Task<IEnumerable<Advertisement>> GetAllAsync()
        {
            return await _context.Advertisements
                .Where(a => a.IsActive)
                .ToListAsync();
        }

        public async Task<Advertisement?> GetByIdAsync(Guid id)
        {
            return await _context.Advertisements
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Advertisement?> GetRandomAsync()
        {
            return await _context.Advertisements
                .Where(a => a.IsActive)
                .OrderBy(x => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        public async Task DisableAsync(Guid id)
        {
            var ad = await _context.Advertisements
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ad == null)
                throw new InvalidOperationException("Ad not found");

            ad.IsActive = false;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var ad = await _context.Advertisements
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ad == null)
                throw new InvalidOperationException("Advertisement not found");

            await _files.DeleteAsync(ObjectId.Parse(ad.AudioFileId));

            await _files.DeleteAsync(ObjectId.Parse(ad.ImageFileId));

            _context.Advertisements.Remove(ad);

            await _context.SaveChangesAsync();
        }
    }
}
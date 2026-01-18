using Microsoft.EntityFrameworkCore;
using MussicApp.Data;
using MussicApp.Models;

namespace MussicApp.Services
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _db;

        public CommentService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Comments> AddAsync(Guid userId, CreateCommentDto dto)
        {
            var comment = new Comments
            {
                UserId = userId,
                TrackId = dto.TrackId,
                Text = dto.Text
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            return comment;
        }

        public async Task<List<Comments>> GetByTrackIdAsync(Guid trackId)
        {
            return await _db.Comments
                .Where(c => c.TrackId == trackId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Comments>> GetByUserIdAsync(Guid userId)
        {
            return await _db.Comments
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(Guid commentId, Guid userId)
        {
            var comment = await _db.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

            if (comment == null)
                return false;

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}

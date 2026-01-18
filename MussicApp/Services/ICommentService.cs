using MussicApp.Models;

namespace MussicApp.Services
{
    public interface ICommentService
    {
        Task<Comments> AddAsync(Guid userId, CreateCommentDto dto);
        Task<List<Comments>> GetByTrackIdAsync(Guid trackId);
        Task<List<Comments>> GetByUserIdAsync(Guid userId);
        Task<bool> DeleteAsync(Guid commentId, Guid userId);
    }
}


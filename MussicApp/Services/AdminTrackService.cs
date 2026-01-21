using Microsoft.EntityFrameworkCore;
using MussicApp.Data;
using MussicApp.Models;

namespace MussicApp.Services
{
    public class AdminTrackService : IAdminTrackService
    {
        private readonly AppDbContext _context;

        public AdminTrackService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Track>> GetPendingAsync()
        {
            return await _context.Tracks
                .Include(t => t.Artist)
                .Include(t => t.Owner)
                .Where(t => t.Status == TrackStatus.Pending)
                .OrderBy(t => t.UploadedAt)
                .ToListAsync();
        }

        public async Task ApproveAsync(Guid trackId)
        {
            var track = await _context.Tracks.FindAsync(trackId)
                ?? throw new Exception("Track not found");

            track.Status = TrackStatus.Approved;
            await _context.SaveChangesAsync();
        }

        public async Task RejectAsync(Guid trackId)
        {
            var track = await _context.Tracks.FindAsync(trackId)
                ?? throw new Exception("Track not found");

            track.Status = TrackStatus.Rejected;
            await _context.SaveChangesAsync();
        }
    }

}

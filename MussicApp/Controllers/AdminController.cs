using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MussicApp.Data;
using MussicApp.Models;
using MussicApp.Services;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminTrackService _service;
    private readonly AppDbContext _db;

    public AdminController(IAdminTrackService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    private class AdminTrackDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string ArtistName { get; set; } = null!;
        public string OwnerUsername { get; set; } = null!;
        public TrackStatus Status { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var tracks = await _service.GetPendingAsync();

        var result = tracks.Select(t => new AdminTrackDto
        {
            Id = t.Id,
            Title = t.Title,
            ArtistName = t.Artist?.Name ?? "Unknown",
            OwnerUsername = t.Owner.Username,
            Status = t.Status,
            UploadedAt = t.UploadedAt
        });

        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _service.ApproveAsync(id);
        return Ok(new { message = "Track approved" });
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        await _service.RejectAsync(id);
        return Ok(new { message = "Track rejected" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/author-requests")]
    public async Task<IActionResult> GetAuthorRequests()
    {
        var requests = await _db.AuthorRequests
            .Include(r => r.User)
            .Where(r => r.Status == AuthorRequestStatus.Pending)
            .ToListAsync();

        return Ok(requests.Select(r => new
        {
            r.Id,
            r.UserId,
            CurrentUsername = r.User.Username,
            RequestedUsername = r.RequestedUsername,
            r.CreatedAt
        }));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/author-requests/{id:guid}/approve")]
    public async Task<IActionResult> ApproveAuthor(Guid id)
    {
        var request = await _db.AuthorRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null) return NotFound();

        if (request.User.Username != request.RequestedUsername)
        {
            request.User.Username = request.RequestedUsername;

            var artist = await _db.Artists.FindAsync(request.UserId);
            if (artist != null)
                artist.Name = request.RequestedUsername;
        }

        request.User.Role = UserRole.Artist;
        request.Status = AuthorRequestStatus.Approved;

        await _db.SaveChangesAsync();
        return Ok("Author approved");
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("admin/author-requests/{id:guid}/reject")]
    public async Task<IActionResult> RejectAuthor(Guid id)
    {
        var request = await _db.AuthorRequests.FindAsync(id);
        if (request == null) return NotFound();

        request.Status = AuthorRequestStatus.Rejected;
        await _db.SaveChangesAsync();

        return Ok("Author request rejected");
    }


}

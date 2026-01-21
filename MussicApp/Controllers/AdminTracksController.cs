using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MussicApp.Models;
using MussicApp.Services;

[ApiController]
[Route("api/admin/tracks")]
[Authorize(Roles = "Admin")]
public class AdminTracksController : ControllerBase
{
    private readonly IAdminTrackService _service;

    public AdminTracksController(IAdminTrackService service)
    {
        _service = service;
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
}

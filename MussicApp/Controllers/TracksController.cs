using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MussicApp.Services;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class TracksController : ControllerBase
{
    private readonly ITrackService _trackService;
    private readonly IFileStorageService _fileStorage;

    public TracksController(
        ITrackService trackService,
        IFileStorageService fileStorage)
    {
        _trackService = trackService;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _trackService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var track = await _trackService.GetByIdAsync(id);
        if (track == null) return NotFound();
        return Ok(track);
    }

    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
    [FromForm] IFormFile file,
    [FromForm] string title,
    [FromForm] IFormFile cover,
    [FromForm] string? albumId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Audio file is required");

        if (cover == null || cover.Length == 0)
            return BadRequest("Cover image is required");

        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("Title is required");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var username = User.Identity!.Name!;

        var track = await _trackService.AddTrackAsync(
            file,
            cover,
            title,
            username,
            albumId,
            userId
        );

        return CreatedAtAction(nameof(Get), new { id = track.Id }, track);
    }

    [HttpGet("{id}/cover")]
    public async Task<IActionResult> GetCover(string id)
    {
        var track = await _trackService.GetByIdAsync(id);
        if (track == null)
            return NotFound("Track not found");

        if (string.IsNullOrEmpty(track.CoverFileId))
            return NotFound("Cover not found");

        var (stream, contentType) =
            await _fileStorage.DownloadAsync(ObjectId.Parse(track.CoverFileId));

        return File(stream, contentType ?? "image/jpeg");
    }


    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var track = await _trackService.GetByIdAsync(id);
        if (track == null)
            return NotFound();

        if (track.OwnerId != userId)
            return Forbid();

        await _trackService.DeleteAsync(track);
        return NoContent();
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> MyTracks()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _trackService.GetByOwnerIdAsync(userId));
    }


    [HttpGet("stream/{id}")]
    public async Task<IActionResult> Stream(string id)
    {
        var track = await _trackService.GetByIdAsync(id);
        if (track == null) return NotFound();

        var (stream, contentType) =
            await _fileStorage.DownloadAsync(ObjectId.Parse(track.FileId));

        return File(stream, contentType, enableRangeProcessing: true);
    }
}

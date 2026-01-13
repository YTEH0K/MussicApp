using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MussicApp.Services;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class TracksController : ControllerBase
{
    private readonly ITrackService _tracks;
    private readonly IFileStorageService _files;

    public TracksController(
        ITrackService tracks,
        IFileStorageService files)
    {
        _tracks = tracks;
        _files = files;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _tracks.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var track = await _tracks.GetByIdAsync(id);
        if (track == null) return NotFound();
        return Ok(track);
    }

    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] IFormFile cover,
        [FromForm] string title,
        [FromForm] Guid artistId,
        [FromForm] Guid? albumId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Audio file is required");

        if (cover == null || cover.Length == 0)
            return BadRequest("Cover image is required");

        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("Title is required");

        var userId = Guid.Parse(
    User.FindFirstValue(ClaimTypes.NameIdentifier)!
);

        var track = await _tracks.CreateAsync(
            file,
            cover,
            title,
            userId,    // ✅ artistId = userId
            albumId,
            userId     // ✅ ownerId
        );

        return CreatedAtAction(
            nameof(Get),
            new { id = track.Id },
            track);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ownerId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var track = await _tracks.GetByIdAsync(id);
        if (track == null)
            return NotFound();

        if (track.OwnerId != ownerId)
            return Forbid();

        await _tracks.DeleteAsync(track);
        return NoContent();
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> MyTracks()
    {
        var ownerId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        return Ok(await _tracks.GetByOwnerIdAsync(ownerId));
    }


    [HttpGet("stream/{id:guid}")]
    public async Task<IActionResult> Stream(Guid id)
    {
        var track = await _tracks.GetByIdAsync(id);
        if (track == null) return NotFound();

        var (stream, contentType) =
            await _files.DownloadAsync(
                ObjectId.Parse(track.FileId));

        return File(
            stream,
            contentType ?? "audio/mpeg",
            enableRangeProcessing: true);
    }

    [HttpGet("{id:guid}/cover")]
    public async Task<IActionResult> GetCover(Guid id)
    {
        var track = await _tracks.GetByIdAsync(id);
        if (track?.CoverFileId == null)
            return NotFound();

        var (stream, contentType) =
            await _files.DownloadAsync(
                ObjectId.Parse(track.CoverFileId));

        return File(stream, contentType ?? "image/jpeg");
    }

    [HttpGet("artists")]
    public async Task<IActionResult> GetArtists()
    {
        var artists = await _tracks.GetAllArtistsAsync();
        return Ok(artists);
    }

    [HttpGet("{id:guid}/artist")]
    public async Task<IActionResult> GetArtist(Guid id)
    {
        var track = await _tracks.GetByIdAsync(id);
        if (track == null) return NotFound();

        var artist = track.Artist;
        return Ok(artist);
    }


}

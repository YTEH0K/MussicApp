using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MussicApp.Services;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AlbumController : ControllerBase
{
    private readonly IAlbumService _albums;

    public AlbumController(IAlbumService albums)
    {
        _albums = albums;
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromForm] string title,
        [FromForm] IFormFile? cover)
    {
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("Title is required");

        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var album = await _albums.CreateAsync(
            title,
            User.Identity!.Name!,
            ownerId,
            cover
        );

        return Ok(album);
    }

    [Authorize]
    [HttpPost("{id:guid}/upload-cover")]
    public async Task<IActionResult> UploadCover(Guid id, [FromForm] IFormFile cover)
    {
        if (cover == null) return BadRequest("Cover file is required.");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var album = await _albums.GetByIdAsync(id);

        if (album.OwnerId != userId)
            return Forbid("You are not the owner of this album.");

        var success = await _albums.AddCoverAsync(id, cover);
        if (!success) return NotFound("Album not found");

        return Ok(new { message = "Cover uploaded successfully" });
    }

    [Authorize]
    [HttpPost("{id:guid}/add-track")]
    public async Task<IActionResult> AddTrack(
    Guid id,
    [FromBody] AddTrackToAlbumDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TrackId))
            return BadRequest("TrackId is required");

        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var album = await _albums.GetByIdAsync(id);

        if (album.OwnerId != ownerId)
            return Forbid("You are not the owner of this album.");


        var success = await _albums.AddTrackAsync(id, Guid.Parse(dto.TrackId));
        if (!success)
            return NotFound("Album not found");

        return Ok(new { message = "Track added to album successfully" });
    }

    [HttpGet("{id:guid}/tracks")]
    public async Task<IActionResult> GetTracks(Guid id)
    {
        var tracks = await _albums.GetTracksAsync(id);
        return Ok(tracks);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var album = await _albums.GetByIdAsync(id);
        return album == null ? NotFound() : Ok(album);
    }

    [HttpGet("all/bum")]
    public async Task<IActionResult> GetAll()
    {
        var albums = await _albums.GetAllAsync();
        return Ok(albums);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyAlbums()
    {
        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var albums = await _albums.GetByOwnerAsync(ownerId);
        return Ok(albums);
    }


    [HttpGet("{id:guid}/cover")]
    public async Task<IActionResult> GetCover(Guid id)
    {
        var (data, contentType) = await _albums.GetCoverAsync(id);
        if (data == null) return NotFound();

        return File(data, contentType ?? "application/octet-stream");
    }
}

public class CreateAlbumDto
{
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
}

public class AddTrackToAlbumDto
{
    public string TrackId { get; set; } = string.Empty;
}
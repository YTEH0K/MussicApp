using Microsoft.AspNetCore.Mvc;
using MussicApp.Services;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class AlbumController : ControllerBase
{
    private readonly IAlbumService _albums;

    public AlbumController(IAlbumService albums)
    {
        _albums = albums;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] string title, [FromForm] string artist)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(artist))
            return BadRequest("Title and Artist are required.");

        var album = await _albums.CreateAsync(title, artist);
        return Ok(album);
    }

    [HttpPost("{id}/upload-cover")]
    public async Task<IActionResult> UploadCover(string id, [FromForm] IFormFile cover)
    {
        if (cover == null) return BadRequest("Cover file is required.");

        var success = await _albums.AddCoverAsync(id, cover);
        if (!success) return NotFound("Album not found");

        return Ok(new { message = "Cover uploaded successfully" });
    }

    [HttpPost("{id}/add-track")]
    public async Task<IActionResult> AddTrack(
    string id,
    [FromBody] AddTrackToAlbumDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TrackId))
            return BadRequest("TrackId is required");

        var success = await _albums.AddTrackAsync(id, dto.TrackId);

        if (!success)
            return NotFound("Album not found");

        return Ok(new { message = "Track added to album successfully" });
    }

    [HttpGet("{id}/tracks")]
    public async Task<IActionResult> GetTracks(string id)
    {
        var tracks = await _albums.GetTracksAsync(id);
        return Ok(tracks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
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

    [HttpGet("{id}/cover")]
    public async Task<IActionResult> GetCover(string id)
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
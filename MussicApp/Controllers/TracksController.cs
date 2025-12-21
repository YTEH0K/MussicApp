using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MussicApp.Services;

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

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string title,
        [FromForm] string artist,
        [FromForm] IFormFile? cover,
        [FromForm] string? albumId)
    {
        var track = await _trackService.AddTrackAsync(
            file, cover, title, artist, albumId);

        return CreatedAtAction(nameof(Get), new { id = track.Id }, track);
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

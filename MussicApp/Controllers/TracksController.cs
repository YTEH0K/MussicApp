using Microsoft.AspNetCore.Mvc;
using MussicApp.Models;
using MussicApp.Services;


[ApiController]
[Route("api/[controller]")]
public class TracksController : ControllerBase
{
    private readonly ITrackService _trackService;


    public TracksController(ITrackService trackService)
    {
        _trackService = trackService;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _trackService.GetAllAsync());
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var track = await _trackService.GetByIdAsync(id);
        if (track == null) return NotFound();
        return Ok(track);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string title, [FromForm] string artist, [FromForm] string album)
    {

        Console.WriteLine($"File received: {file?.FileName}, size: {file?.Length}");

        if (file == null || file.Length == 0) return BadRequest("File required");


        var track = await _trackService.AddTrackAsync(file, title, artist, album);
        return CreatedAtAction(nameof(Get), new { id = track.Id }, track);
    }


    [HttpGet("stream/{id}")]
    public async Task<IActionResult> Stream(int id)
    {
        var track = await _trackService.GetByIdAsync(id);
        if (track == null) return NotFound();


        return File(track.FileData, track.FileType, enableRangeProcessing: true);
    }
}
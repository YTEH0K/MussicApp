using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MussicApp.Models;
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

    private static TrackDto ToDto(Track track)
    {
        return new TrackDto
        {
            Id = track.Id,
            Title = track.Title,
            Lyrics = track.Lyrics,
            ArtistId = track.ArtistId,
            ArtistName = track.Artist?.Name,
            OwnerId = track.OwnerId,
            AlbumId = track.AlbumId,
            FileId = track.FileId,
            CoverFileId = track.CoverFileId,
            Duration = track.Duration,
            UploadedAt = track.UploadedAt,
            Status = track.Status.ToString()
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tracks = await _tracks.GetAllAsync();
        return Ok(tracks.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var track = await _tracks.GetByIdAsync(id);
        if (track == null) return NotFound();

        return Ok(ToDto(track));
    }

    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] IFormFile cover,
        [FromForm] string title,
        [FromForm] string lyrics,
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
            lyrics,
            artistId,
            albumId,
            userId
        );

        return CreatedAtAction(
            nameof(Get),
            new { id = track.Id },
            ToDto(track)
        );
    }

    [Authorize]
    [HttpPut("lyric/{id:guid}")]
    public async Task<IActionResult> UpdateLyrics(
        Guid id,
        [FromBody] LyricsDto dto)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var track = await _tracks.GetByIdAsync(id);
        if (track == null) return NotFound();
        if (track.OwnerId != userId) return Forbid();

        track.Lyrics = dto.Lyrics;
        await _tracks.UpdateAsync(track);

        return Ok(ToDto(track));
    }

    [HttpGet("lyric/{id:guid}")]
    public async Task<IActionResult> GetLyrics(Guid id)
    {
        var track = await _tracks.GetByIdAsync(id);
        if (track == null) return NotFound();

        if (string.IsNullOrWhiteSpace(track.Lyrics))
            return NotFound("Lyrics not found");

        return Ok(new { lyrics = track.Lyrics });
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var track = await _tracks.GetByIdAsync(id);
        if (track == null) return NotFound();
        if (track.OwnerId != userId) return Forbid();

        await _tracks.DeleteAsync(track);
        return NoContent();
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> MyTracks()
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var tracks = await _tracks.GetByOwnerIdAsync(userId);
        return Ok(tracks.Select(ToDto));
    }

    [HttpGet("stream/{id:guid}")]
    public async Task<IActionResult> Stream(Guid id)
    {
        var track = await _tracks.GetByIdAsync(id);
        if (track == null) return NotFound();

        var (stream, contentType) =
            await _files.DownloadAsync(
                ObjectId.Parse(track.FileId)
            );

        return File(
            stream,
            contentType ?? "audio/mpeg",
            enableRangeProcessing: true
        );
    }

    [HttpGet("{id:guid}/cover")]
    public async Task<IActionResult> GetCover(Guid id)
    {
        var track = await _tracks.GetByIdAsync(id);
        if (track?.CoverFileId == null) return NotFound();

        var (stream, contentType) =
            await _files.DownloadAsync(
                ObjectId.Parse(track.CoverFileId)
            );

        return File(stream, contentType ?? "image/jpeg");
    }

    [HttpGet("artists")]
    public async Task<IActionResult> GetArtists()
    {
        var artists = await _tracks.GetAllArtistsAsync();

        return Ok(artists.Select(a => new ArtistDto
        {
            Id = a.Id,
            Name = a.Name
        }));
    }

    [HttpGet("{id:guid}/artist")]
    public async Task<IActionResult> GetArtist(Guid id)
    {
        var track = await _tracks.GetByIdAsync(id);
        if (track?.Artist == null) return NotFound();

        return Ok(new ArtistDto
        {
            Id = track.Artist.Id,
            Name = track.Artist.Name
        });
    }
}

public class TrackDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Lyrics { get; set; }

    public Guid ArtistId { get; set; }
    public string? ArtistName { get; set; }

    public Guid OwnerId { get; set; }
    public Guid? AlbumId { get; set; }

    public string FileId { get; set; } = null!;
    public string? CoverFileId { get; set; }

    public TimeSpan Duration { get; set; }
    public DateTime UploadedAt { get; set; }

    public string Status { get; set; } = null!;
}

public class LyricsDto
{
    public string Lyrics { get; set; } = null!;
}

public class ArtistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

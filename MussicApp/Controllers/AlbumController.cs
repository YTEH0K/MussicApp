using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MussicApp.Models;

[ApiController]
[Route("api/[controller]")]
public class AlbumController : ControllerBase
{
    private readonly AppDbContext _db;

    public AlbumController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromForm] string title,
        [FromForm] string artist,
        [FromForm] IFormFile? cover)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(artist))
            return BadRequest("Title and artist are required.");

        var album = new Album
        {
            Title = title,
            Artist = artist
        };

        if (cover != null)
        {
            using var ms = new MemoryStream();
            await cover.CopyToAsync(ms);
            album.CoverData = ms.ToArray();
            album.CoverType = cover.ContentType;
        }

        _db.Albums.Add(album);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Album created successfully",
            album.Id,
            album.Title,
            album.Artist,
            HasCover = cover != null
        });
    }

    [HttpPost("add-track")]
    public async Task<IActionResult> AddTrackToAlbum([FromForm] int albumId, [FromForm] int trackId)
    {
        var album = await _db.Albums
            .Include(a => a.AlbumTracks)
            .FirstOrDefaultAsync(a => a.Id == albumId);

        var track = await _db.Tracks.FindAsync(trackId);

        if (album == null) return NotFound("Album not found");
        if (track == null) return NotFound("Track not found");

        if (!album.AlbumTracks.Any(at => at.TrackId == trackId))
        {
            album.AlbumTracks.Add(new AlbumTrack { AlbumId = albumId, TrackId = trackId });
            await _db.SaveChangesAsync();
        }

        return Ok(new
        {
            message = "Track added to album",
            albumId,
            trackId
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAlbum(int id)
    {
        var album = await _db.Albums
            .Include(a => a.AlbumTracks)
                .ThenInclude(at => at.Track)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (album == null) return NotFound();

        return Ok(new
        {
            album.Id,
            album.Title,
            album.Artist,
            HasCover = album.CoverData != null,
            Tracks = album.AlbumTracks.Select(at => new
            {
                at.Track.Id,
                at.Track.Title,
                at.Track.Artist,
                at.Track.Duration,
                HasCover = at.Track.CoverData != null
            })
        });
    }

}

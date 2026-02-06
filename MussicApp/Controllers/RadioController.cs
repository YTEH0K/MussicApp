using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MussicApp.Models;
using MussicApp.Services;
using System.Security.Claims;

namespace MussicApp.Controllers
{
    [ApiController]
    [Route("api/radio")]
    [Authorize]
    public class RadioController : ControllerBase
    {
        private readonly IRadioService _radio;
        private readonly ITrackService _tracks;

        public RadioController(IRadioService radio, ITrackService tracks)
        {
            _tracks = tracks;
            _radio = radio;
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
                Status = track.Status.ToString(),
                Genres = track.TrackGenres?
                    .Select(tg => tg.Genre?.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList()
            };
        }

        [HttpGet("{seedTrackId:guid}")]
        public async Task<IActionResult> GetRadio(
            Guid seedTrackId,
            [FromQuery] int limit = 25)
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var queue = await _radio.BuildRadioQueueAsync(
                seedTrackId,
                userId,
                limit
            );

            return Ok(queue);
        }

        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> MyListeningHistory(
       [FromQuery] int limit = 50)
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var history = await _tracks.GetListeningHistoryAsync(userId, limit);

            var result = history.Select(h => new ListeningHistoryDto
            {
                Track = ToDto(h.Track),
                PlayedAt = h.PlayedAt,
                PlayedSeconds = h.PlayedDuration.TotalSeconds
            });

            return Ok(result);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentlyPlayed(
        [FromQuery] int source = 50,
        [FromQuery] int limit = 20)
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _radio.GetRandomRecentlyPlayedAsync(
                userId,
                source,
                limit
            );

            return Ok(result);
        }

        [Authorize]
        [HttpPost("favorite-genres")]
        public async Task<IActionResult> SetFavoriteGenres(
        [FromBody] SelectFavoriteGenresDto dto)
        {
            if (dto.GenreIds == null || dto.GenreIds.Count == 0)
                return BadRequest("Select at least one genre");

            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            try
            {
                await _tracks.SetUserFavoriteGenresAsync(
                    userId,
                    dto.GenreIds
                );

                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }

}


public class ListeningHistoryDto
{
    public TrackDto Track { get; set; } = null!;
    public DateTime PlayedAt { get; set; }
    public double PlayedSeconds { get; set; }
}

public class SelectFavoriteGenresDto
{
    public List<Guid> GenreIds { get; set; } = [];
}
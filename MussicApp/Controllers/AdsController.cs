using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;using MussicApp.Models.Other;
using MussicApp.Services.Advertisements;

[ApiController]
[Route("api/ads")]
public class AdsController : ControllerBase
{
    private readonly IAdService _ads;
    private readonly IFileStorageService _files;

    public AdsController(
        IAdService ads,
        IFileStorageService files)
    {
        _ads = ads;
        _files = files;
    }

    private AdvertisementDto ToDto(Advertisement ad)
    {
        return new AdvertisementDto
        {
            Id = ad.Id,
            Title = ad.Title,
            TargetUrl = ad.TargetUrl,
            ImageUrl = $"/api/ads/{ad.Id}/image",
            AudioUrl = $"/api/ads/{ad.Id}/audio"
        };
    }

    [Authorize(Roles = "admin")]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile image,
        [FromForm] IFormFile audio,
        [FromForm] string title,
        [FromForm] string? targetUrl,
        [FromForm] int durationSeconds)
    {
        var ad = await _ads.UploadAsync(
            image,
            audio,
            title,
            targetUrl);

        return Ok(ToDto(ad));
    }

    [HttpGet("random")]
    public async Task<IActionResult> Random()
    {
        var ad = await _ads.GetRandomAsync();

        if (ad == null)
            return NoContent();

        return Ok(ToDto(ad));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ads = await _ads.GetAllAsync();

        return Ok(ads.Select(ToDto));
    }

    [HttpGet("{id:guid}/image")]
    public async Task<IActionResult> GetImage(Guid id)
    {
        var ad = await _ads.GetByIdAsync(id);

        if (ad == null)
            return NotFound();

        var (stream, contentType) =
            await _files.DownloadAsync(
                ObjectId.Parse(ad.ImageFileId));

        return File(stream, contentType ?? "image/jpeg");
    }

    [HttpGet("{id:guid}/audio")]
    public async Task<IActionResult> StreamAudio(Guid id)
    {
        var ad = await _ads.GetByIdAsync(id);

        if (ad == null)
            return NotFound();

        var (stream, contentType) =
            await _files.DownloadAsync(
                ObjectId.Parse(ad.AudioFileId));

        return File(
            stream,
            contentType ?? "audio/mpeg",
            enableRangeProcessing: true);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:guid}/disable")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await _ads.DisableAsync(id);

        return NoContent();
    }
}
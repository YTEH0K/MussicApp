using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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

    [Authorize(Roles = "admin")]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile image,
        [FromForm] string title,
        [FromForm] string? targetUrl)
    {
        var ad = await _ads.UploadAsync(
            image,
            title,
            targetUrl);

        return Ok(ad);
    }

    [HttpGet("random")]
    public async Task<IActionResult> GetRandom()
    {
        var ad = await _ads.GetRandomAsync();

        if (ad == null)
            return NoContent();

        return Ok(new
        {
            ad.Id,
            ad.Title,
            ad.TargetUrl,
            imageUrl = $"/api/ads/{ad.Id}/image"
        });
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
}
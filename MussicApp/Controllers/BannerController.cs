using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MussicApp.Services;
using MussicApp.Services.Banner;

[ApiController]
[Route("api/banners")]
public class BannerController : ControllerBase
{
    private readonly IBannerService _banners;

    public BannerController(IBannerService banners)
    {
        _banners = banners;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile image,
        [FromForm] string title,
        [FromForm] string? link)
    {
        var banner = await _banners.UploadAsync(image, title, link);

        return Ok(banner);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var banners = await _banners.GetAllAsync();

        return Ok(banners);
    }

    [HttpGet("image/{id:guid}")]
    public async Task<IActionResult> GetImage(Guid id)
    {
        var image = await _banners.GetImageAsync(id);

        if (image == null)
            return NotFound();

        return File(image.Value.Data, image.Value.ContentType);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _banners.DeleteAsync(id);

        return Ok("Banner deleted");
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MussicApp.Models;
using MussicApp.Services;

[ApiController]
[Route("api/[controller]")]
public class IconController : ControllerBase
{
    private readonly IIconService _iconService;

    public IconController(IIconService iconService)
    {
        _iconService = iconService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        try
        {
            var icon = await _iconService.UploadIconAsync(file);
            return Ok(icon);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [AllowAnonymous]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var icons = await _iconService.GetAllIconsAsync();
        return Ok(icons);
    }

    [AllowAnonymous]
    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetByFileName(string fileName)
    {
        var icon = await _iconService.GetByFileNameAsync(fileName);
        if (icon == null) return NotFound();
        return Ok(icon);
    }

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> Delete(string fileName)
    {
        await _iconService.DeleteIconAsync(fileName);
        return Ok("Icon deleted");
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MussicApp.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest("Username already taken");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Password = dto.Password
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "User successfully registered" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null)
            return Unauthorized(new { message = "User not found" });

        if (user.Password != dto.Password)
            return Unauthorized(new { message = "Invalid password" });

        return Ok(new { message = "Logged in successfully" });
    }
}

public record RegisterDto(
    string Username,
    string Email,
    string PhoneNumber,
    string Password
);

public record LoginDto(
    string Username,
    string Password
);

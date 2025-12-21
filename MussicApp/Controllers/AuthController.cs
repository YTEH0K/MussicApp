using BCrypt.Net;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using MussicApp.Models;
using MussicApp.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _users;

    public AuthController(IUserService users)
    {
        _users = users;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _users.ExistsByEmailAsync(dto.Email))
            return BadRequest("Email already registered");

        if (await _users.ExistsByUsernameAsync(dto.Username))
            return BadRequest("Username already taken");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Provider = AuthProvider.Local
        };

        await _users.CreateAsync(user);

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email);

        if (user == null || user.Provider != AuthProvider.Local)
            return Unauthorized("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        return Ok(new
        {
            message = "Logged in successfully",
            user.Id,
            user.Username,
            user.Email
        });
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(GoogleAuthDto dto)
    {
        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
        }
        catch
        {
            return Unauthorized("Invalid Google token");
        }

        var user = await _users.GetByGoogleIdAsync(payload.Subject);

        if (user == null)
        {
            var baseUsername = payload.Name?.Replace(" ", "").ToLower()
                               ?? payload.Email.Split('@')[0];

            var username = baseUsername;
            var i = 1;

            while (await _users.ExistsByUsernameAsync(username))
                username = $"{baseUsername}{i++}";

            user = new User
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                Username = username,
                Provider = AuthProvider.Google
            };

            await _users.CreateAsync(user);
        }

        return Ok(new
        {
            message = "Google login successful",
            user.Id,
            user.Username,
            user.Email
        });
    }
}


public record RegisterDto(
    string Username,
    string Email,
    string PhoneNumber,
    string Password
);

public record LoginDto(
    string Email,
    string Password
);

public class GoogleAuthDto
{
    public string IdToken { get; set; } = string.Empty;
}


using BCrypt.Net;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using MussicApp.Models;
using MussicApp.Services;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _users;
    private readonly JwtService _jwt;
    private readonly IEmailService _email;

    public AuthController(IUserService users, JwtService jwt, IEmailService email)
    {
        _users = users;
        _jwt = jwt;
        _email = email;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _users.ExistsByEmailAsync(dto.Email))
            return BadRequest("Email already registered");

        if (await _users.ExistsByUsernameAsync(dto.Username))
            return BadRequest("Username already taken");

        var code = RandomNumberGenerator
            .GetInt32(100000, 999999)
            .ToString();

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Provider = AuthProvider.Local,

            EmailConfirmed = false,
            EmailConfirmCode = code,
            EmailConfirmExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        await _users.CreateAsync(user);

        await _email.SendEmailAsync(
            user.Email,
            "Confirm your MussicApp account",
            $"""
        <h2>Email confirmation</h2>
        <p>Your confirmation code:</p>
        <h1>{code}</h1>
        <p>This code expires in 10 minutes.</p>
        """
        );

        return Ok("Confirmation code sent to email");
    }


    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmRegister(ConfirmEmailDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email);

        if (user == null ||
            user.EmailConfirmCode != dto.Code ||
            user.EmailConfirmExpiresAt < DateTime.UtcNow)
            return BadRequest("Invalid or expired code");

        user.EmailConfirmed = true;
        user.EmailConfirmCode = null;
        user.EmailConfirmExpiresAt = null;

        await _users.UpdateAsync(user);

        var token = _jwt.GenerateToken(user);

        return Ok(new
        {
            token,
            user.Username,
            user.Email
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email);

        if (user != null)
        {
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            user.PasswordResetCode = code;
            user.PasswordResetExpiresAt = DateTime.UtcNow.AddMinutes(15);

            await _users.UpdateAsync(user);

        }

        return Ok("If this email exists, instructions were sent");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email);

        if (user == null ||
            user.PasswordResetCode != dto.Code ||
            user.PasswordResetExpiresAt < DateTime.UtcNow)
            return BadRequest("Invalid or expired code");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetCode = null;
        user.PasswordResetExpiresAt = null;

        await _users.UpdateAsync(user);

        return Ok("Password updated");
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email);

        if (user == null || user.Provider != AuthProvider.Local)
            return Unauthorized("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        if (!user.EmailConfirmed)
            return Unauthorized("Confirm your email first");

        var token = _jwt.GenerateToken(user);

        return Ok(new
        {
            token,
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
                Provider = AuthProvider.Google,
                EmailConfirmed = true
            };

            await _users.CreateAsync(user);
        }

        var token = _jwt.GenerateToken(user);

        return Ok(new
        {
            token,
            user.Username,
            user.Email
        });
    }

    [Authorize]
    [HttpPost("like")]
    public async Task<IActionResult> LikeTrack(LikeTrackDto dto)
    {
        var userId = User.FindFirst("id")?.Value;
        if (userId == null) return Unauthorized();

        await _users.AddLikeAsync(userId, dto.TrackId);
        return Ok("Track liked");
    }

    [Authorize]
    [HttpPost("unlike")]
    public async Task<IActionResult> UnlikeTrack(LikeTrackDto dto)

    {
        var userId = User.FindFirst("id")?.Value;
        if (userId == null) return Unauthorized();

        await _users.RemoveLikeAsync(userId, dto.TrackId);
        return Ok("Track unliked");
    }

    [Authorize]
    [HttpGet("liked-tracks")]
    public async Task<IActionResult> GetLikedTracks()

    {
        var userId = User.FindFirst("id")?.Value;
        if (userId == null) return Unauthorized();

        var likedTrackIds = await _users.GetLikedTrackIdsAsync(userId);
        return Ok(likedTrackIds);
    }

    [Authorize]
    [HttpPost("change-avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ChangeAvatar([FromForm] IFormFile avatar)
    {
        var userId = User.FindFirst("id")?.Value;
        if (userId == null) return Unauthorized();

        if (avatar == null || avatar.Length == 0)
            return BadRequest("Avatar file is required");

        await _users.SetAvatarAsync(userId, avatar);
        return Ok("Avatar updated");
    }

    [HttpGet("avatar/{userId}")]
    public async Task<IActionResult> GetAvatar(string userId)
    {
        var avatar = await _users.GetAvatarAsync(userId);
        if (avatar == null) return NotFound();

        return File(avatar.Value.Data, avatar.Value.ContentType);
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
public record ConfirmEmailDto(string Email, string Code);

public record ForgotPasswordDto(string Email);

public record LikeTrackDto(string TrackId);

public record ResetPasswordDto(
    string Email,
    string Code,
    string NewPassword
);

public class GoogleAuthDto
{
    public string IdToken { get; set; } = string.Empty;
}


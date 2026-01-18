using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MussicApp.Models;
using MussicApp.Services;
using System.Security.Claims;

namespace MussicApp.Controllers
{
    [ApiController]
    [Route("api/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _comments;

        public CommentsController(ICommentService comments)
        {
            _comments = comments;
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var comment = await _comments.AddAsync(Guid.Parse(userId), dto);
            return Ok(comment);
        }

        [HttpGet("test")]
        [Authorize]
        public IActionResult Test()
        {
            return Ok(new { msg = "Authorized!", user = User.Identity?.Name });
        }

        [HttpGet("track/{trackId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByTrack(Guid trackId)
        {
            return Ok(await _comments.GetByTrackIdAsync(trackId));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            return Ok(await _comments.GetByUserIdAsync(Guid.Parse(userId)));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var deleted = await _comments.DeleteAsync(id, Guid.Parse(userId));
            if (!deleted) return NotFound();

            return Ok();
        }
    }
}

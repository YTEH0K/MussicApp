using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public RadioController(IRadioService radio)
        {
            _radio = radio;
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

        [HttpGet("history")]
        public async Task<IActionResult> GetMyHistory(
        [FromQuery] int limit = 50)
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var history = await _radio.GetListeningHistoryAsync(
                userId,
                limit
            );

            return Ok(history);
        }

    }

}

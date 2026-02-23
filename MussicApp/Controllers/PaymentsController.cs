using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MussicApp.Services.Other;
using System.Security.Claims;

namespace MussicApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly ISubscriptionService _subs;

        public PaymentsController(ISubscriptionService subs)
        {
            _subs = subs;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create()
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var url = await _subs
                .CreateSubscriptionSessionAsync(userId);

            return Ok(new { url });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(
                HttpContext.Request.Body)
                .ReadToEndAsync();

            var signature =
                Request.Headers["Stripe-Signature"];

            await _subs.HandleWebhookAsync(json, signature);

            return Ok();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniNetwork.Application.Notifications;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MiniNetwork.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        // GET api/notifications?page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _notificationService.GetNotificationsAsync(userId, page, pageSize, ct);
            if (!result.Succeeded || result.Data is null)
                return BadRequest(new { error = result.Error });

            return Ok(result.Data);
        }
        // GET api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _notificationService.GetUnreadCountAsync(userId, ct);
            if (!result.Succeeded)
                return BadRequest(new { error = result.Error });

            return Ok(new { count = result.Data });
        }
        // POST api/notifications/{id}/read
        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _notificationService.MarkAsReadAsync(userId, id, ct);
            if (!result.Succeeded)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
        // POST api/notifications/read-all
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _notificationService.MarkAllAsReadAsync(userId, ct);
            if (!result.Succeeded)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
        private Guid GetUserIdFromClaims()
        {
            var userIdStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User.FindFirstValue("sub");

            return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        }
    }
}

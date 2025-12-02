using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MiniNetwork.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromClaims(Context.User);
        if (userId != Guid.Empty)
        {
            // Join vào group theo userId để gửi notif riêng
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        }
        //TESTING: Gửi tin nhắn ping ngay khi kết nối
        await Clients.Caller.SendAsync("Ping", $"Connected at {DateTime.UtcNow:O}");


        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserIdFromClaims(Context.User);
        if (userId != Guid.Empty)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
        }

        await base.OnDisconnectedAsync(exception);
    }

    private Guid GetUserIdFromClaims(ClaimsPrincipal? user)
    {
        if (user == null) return Guid.Empty;

        var userIdStr =
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            user.FindFirstValue("sub");

        return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
    }
}

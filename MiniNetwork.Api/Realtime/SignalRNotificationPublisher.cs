using Microsoft.AspNetCore.SignalR;
using MiniNetwork.Api.Hubs;
using MiniNetwork.Application.Notifications;
using MiniNetwork.Application.Notifications.DTOs;

namespace MiniNetwork.Api.Realtime;

public class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPublisher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishAsync(
        Guid recipientUserId,
        NotificationDto notification,
        CancellationToken ct = default)
    {
        await _hubContext.Clients
            .Group(recipientUserId.ToString())
            .SendAsync("NotificationReceived", notification, ct);
    }
}

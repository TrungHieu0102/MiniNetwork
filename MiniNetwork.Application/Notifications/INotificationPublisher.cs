using MiniNetwork.Application.Notifications.DTOs;

namespace MiniNetwork.Application.Notifications;

public interface INotificationPublisher
{
    Task PublishAsync(
        Guid recipientUserId,
        NotificationDto notification,
        CancellationToken ct = default);
}

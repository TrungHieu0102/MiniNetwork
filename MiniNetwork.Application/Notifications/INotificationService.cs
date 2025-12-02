using MiniNetwork.Application.Common;
using MiniNetwork.Application.Notifications.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Notifications
{
    public interface INotificationService
    {
        Task<Result<PagedResult<NotificationDto>>> GetNotificationsAsync(
            Guid recipientId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);
        Task<Result<int>> GetUnreadCountAsync(
            Guid userId,
            CancellationToken ct = default);
        Task<Result> MarkAsReadAsync(
            Guid notificationId,
            Guid userId,
            CancellationToken ct = default);
        Task<Result> MarkAllAsReadAsync(
            Guid userId,
            CancellationToken ct = default);
        //Fire notification methods
        Task<Result> CreateFollowNotificationAsync(
           Guid actorId,
           Guid targetUserId,
           CancellationToken ct);

        Task<Result> CreatePostLikedNotificationAsync(
            Guid actorId,
            Guid postId,
            CancellationToken ct);

        Task<Result> CreatePostCommentedNotificationAsync(
            Guid actorId,
            Guid postId,
            Guid commentId,
            CancellationToken ct);
    }
}

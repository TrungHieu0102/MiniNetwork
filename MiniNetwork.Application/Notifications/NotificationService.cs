using MiniNetwork.Application.Common;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Application.Notifications.DTOs;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        public readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationPublisher _publisher;   
        public NotificationService(
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            INotificationPublisher publisher)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;

        }

        public async Task<Result<PagedResult<NotificationDto>>> GetNotificationsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return Result<PagedResult<NotificationDto>>.Failure("User id is invalid.");

            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0 || pageSize > 50) pageSize = 20;

            var skip = (pageNumber - 1) * pageSize;

            // get page notifications
            var items = await _notificationRepository.GetNotificationsForUserAsync(userId, skip, pageSize, ct);
            var totalCount = items.Count;

            var dtos = items.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt,
                ActorId = n.ActorId,
                ActorDisplayName = n.Actor.DisplayName,
                ActorAvatarUrl = n.Actor.AvatarUrl,
                PostId = n.PostId,
                CommentId = n.CommentId
            }).ToList();

            var paged = PagedResult<NotificationDto>.Create(dtos, pageNumber, pageSize, totalCount);

            return Result<PagedResult<NotificationDto>>.Success(paged);
        }

        public async Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return Result<int>.Failure("Invalid user id.");

            var count = await _notificationRepository.GetUnreadCountAsync(userId, ct);
            return Result<int>.Success(count);
        }


        public async Task<Result> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
        {
            var notif = await _notificationRepository.GetByIdAsync(notificationId, ct);
            if (notif is null || notif.RecipientId != userId)
                return Result.Failure("Notification không tồn tại.");

            notif.MarkAsRead();
            _notificationRepository.Update(notif);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
        {
            var list = await _notificationRepository.GetNotificationsForUserAsync(userId, 0, 200, ct);
            foreach (var n in list.Where(x => !x.IsRead))
            {
                n.MarkAsRead();
                _notificationRepository.Update(n);
            }
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> CreateFollowNotificationAsync(Guid actorId, Guid targetUserId, CancellationToken ct)
        {
            if (actorId == targetUserId)
                return Result.Success(); // không notif cho self

            var targetUser = await _userRepository.GetByIdAsync(targetUserId, ct);
            if (targetUser is null || targetUser.IsDeleted)
                return Result.Failure("User không tồn tại.");

            var actor = await _userRepository.GetByIdAsync(actorId, ct);
            if (actor is null || actor.IsDeleted)
                return Result.Failure("User không tồn tại.");

            var message = $"{actor.DisplayName} đã follow bạn.";

            var notif = new Notification(
                recipientId: targetUserId,
                actorId: actorId,
                type: NotificationType.Follow,
                message: message,
                postId: null,
                commentId: null
            );

            await _notificationRepository.AddAsync(notif, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            var dto = new NotificationDto
            {
                Id = notif.Id,
                Type = notif.Type.ToString(),
                Message = notif.Message,
                IsRead = notif.IsRead,
                CreatedAt = notif.CreatedAt,
                ActorId = actor.Id,
                ActorDisplayName = actor.DisplayName,
                ActorAvatarUrl = actor.AvatarUrl,
                PostId = notif.PostId,
                CommentId = notif.CommentId
            };

            await _publisher.PublishAsync(targetUserId, dto, ct);

            return Result.Success();
        }

        public Task<Result> CreatePostLikedNotificationAsync(Guid actorId, Guid postId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Result> CreatePostCommentedNotificationAsync(Guid actorId, Guid postId, Guid commentId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}

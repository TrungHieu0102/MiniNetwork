using MiniNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Interfaces.Repositories
{
    public interface INotificationRepository : IRepository<Notification>
    {
         Task<int> GetUnreadCountAsync(
            Guid recipientId,
            CancellationToken ct = default);
        Task<IReadOnlyList<Notification>> GetNotificationsForUserAsync(
            Guid recipientId,
            int skip,
            int take,
            CancellationToken ct = default);
    }
}

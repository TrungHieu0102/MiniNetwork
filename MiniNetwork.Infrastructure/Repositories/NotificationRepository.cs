using Microsoft.EntityFrameworkCore;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Infrastructure.Repositories
{
    public class NotificationRepository : EfRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(MiniNetworkDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<int> GetUnreadCountAsync(Guid recipientId, CancellationToken ct = default)
        {
            return await _dbSet.Where(n => n.RecipientId == recipientId && !n.IsRead).CountAsync(ct);
        }

        public async Task<IReadOnlyList<Notification>> GetNotificationsForUserAsync(Guid recipientId, int skip, int take, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(n => n.RecipientId == recipientId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(n => n.Actor)
                .ToListAsync(ct);
        }
    }
}

using MiniNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Interfaces.Repositories
{
    public interface IFollowRepository : IRepository<Follow>
    {
        Task<Follow?> GetAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default);
        Task<int> GetFollowersCountAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default);

    }
}

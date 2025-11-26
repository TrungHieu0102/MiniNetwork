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
        Task<IReadOnlyList<User>> GetFollowersAsync(Guid userId,string? query, int take, int skip, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<User>> GetFollowingAsync(Guid userId, string? query, int take, int skip, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Guid>> GetFollowingIdsAsync(Guid userId, CancellationToken ct = default);
        Task<IReadOnlyList<Follow>> GetFollowingsByFollowerIdsAsync(IEnumerable<Guid> followerIds, CancellationToken ct = default);
    }
}

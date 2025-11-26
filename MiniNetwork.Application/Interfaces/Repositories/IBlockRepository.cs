using MiniNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Interfaces.Repositories
{
    public interface IBlockRepository : IRepository<Block>
    {
        Task<bool> ExistsAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default);
        Task<bool> IsBlockedBetweenAsync(Guid userAId, Guid userBId, CancellationToken ct = default);
        Task<Block?> GetAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default);
        Task<IReadOnlyList<Guid>> GetBlockedUserIdsForAsync(Guid userId, CancellationToken ct = default);
    }
}

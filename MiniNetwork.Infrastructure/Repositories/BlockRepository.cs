using Microsoft.EntityFrameworkCore;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Infrastructure.Persistence;

namespace MiniNetwork.Infrastructure.Repositories;

public class BlockRepository : EfRepository<Block>, IBlockRepository
{
    public BlockRepository(MiniNetworkDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<bool> ExistsAsync(
        Guid blockerId,
        Guid blockedId,
        CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(
            b => b.BlockerId == blockerId && b.BlockedId == blockedId,
            ct);
    }

    public async Task<Block?> GetAsync(
        Guid blockerId,
        Guid blockedId,
        CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            b => b.BlockerId == blockerId && b.BlockedId == blockedId,
            ct);
    }

    public async Task<bool> IsBlockedBetweenAsync(
        Guid userAId,
        Guid userBId,
        CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(
            b =>
                (b.BlockerId == userAId && b.BlockedId == userBId) ||
                (b.BlockerId == userBId && b.BlockedId == userAId),
            ct);
    }
    public async Task<IReadOnlyList<Guid>> GetBlockedUserIdsForAsync(
    Guid userId,
    CancellationToken ct = default)
    {
        return await _dbSet
            .Where(b => b.BlockerId == userId || b.BlockedId == userId)
            .Select(b => b.BlockerId == userId ? b.BlockedId : b.BlockerId)
            .ToListAsync(ct);
    }
}

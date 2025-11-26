using Microsoft.EntityFrameworkCore;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Infrastructure.Repositories
{
    public class FollowRepository(MiniNetworkDbContext dbContext) : EfRepository<Follow>(dbContext) , IFollowRepository
    {
        public async Task<bool> ExistsAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId, cancellationToken);
        }

        public async Task<Follow?> GetAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId,
                cancellationToken);
        }

        public async Task<IReadOnlyList<User>> GetFollowersAsync(Guid userId, string? query, int take, int skip, CancellationToken cancellationToken = default)
        {
            var usersQuery =
                        from f in _dbSet
                        join u in _dbContext.Users on f.FollowerId equals u.Id
                        where f.FolloweeId == userId && !u.IsDeleted
                        select u;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToUpper();
                usersQuery = usersQuery.Where(u =>
                    u.NormalizedUserName.Contains(q) ||
                    u.DisplayName.ToUpper().Contains(q));
            }

            return await usersQuery
                .OrderByDescending(u => u.CreatedAt) 
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetFollowersCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(f => f.FolloweeId == userId, cancellationToken);
        }

        public async Task<IReadOnlyList<User>> GetFollowingAsync(Guid userId, string? query, int take, int skip, CancellationToken cancellationToken = default)
        {
            var usersQuery = from f in _dbSet
                             join u in _dbContext.Users on f.FollowerId equals u.Id
                             where f.FolloweeId == userId && !u.IsDeleted
                             select u;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToUpper();
                usersQuery = usersQuery.Where(u =>
                    u.NormalizedUserName.Contains(q) ||
                    u.DisplayName.ToUpper().Contains(q));
            }
            return await usersQuery
            .OrderByDescending(u => u.CreatedAt) 
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        }

        public async Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(f => f.FollowerId == userId, cancellationToken);
        }
        public async Task<IReadOnlyList<Guid>> GetFollowingIdsAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbSet.Where(f=>f.FollowerId == id)
                                .Select(f=>f.FolloweeId).ToListAsync(ct);
        }
        public async Task<IReadOnlyList<Follow>> GetFollowingsByFollowerIdsAsync(IEnumerable<Guid> followerIdsIds, CancellationToken ct)
        {
            var ids = followerIdsIds.ToList();
            if (ids.Count == 0) return Array.Empty<Follow>();
            return await _dbSet
                   .Where(f => ids.Contains(f.FollowerId))
                   .ToListAsync(ct);
        }
    }
}

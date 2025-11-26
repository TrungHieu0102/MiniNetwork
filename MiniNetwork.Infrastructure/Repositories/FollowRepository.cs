using Microsoft.EntityFrameworkCore;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Infrastructure.Repositories
{
    public class FollowRepository(MiniNetworkDbContext dbContext) : EfRepository<Follow>(dbContext), IFollowRepository
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
            var blocks = _dbContext.Blocks;

            var usersQuery =
                        from f in _dbSet
                        join u in _dbContext.Users on f.FollowerId equals u.Id
                        where f.FolloweeId == userId && !u.IsDeleted
                         && !blocks.Any(b =>
                        (b.BlockerId == userId && b.BlockedId == u.Id) ||
                        (b.BlockerId == u.Id && b.BlockedId == userId))
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
            var blocks = _dbContext.Blocks;

            var usersQuery = from f in _dbSet
                             join u in _dbContext.Users on f.FollowerId equals u.Id
                             where f.FolloweeId == userId && !u.IsDeleted
                               && !blocks.Any(b =>
                                (b.BlockerId == userId && b.BlockedId == u.Id) ||
                                (b.BlockerId == u.Id && b.BlockedId == userId))
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
            return await _dbSet.Where(f => f.FollowerId == id)
                                .Select(f => f.FolloweeId).ToListAsync(ct);
        }
        public async Task<IReadOnlyList<Follow>> GetFollowingsByFollowerIdsAsync(IEnumerable<Guid> followerIdsIds, CancellationToken ct)
        {
            var ids = followerIdsIds.ToList();
            if (ids.Count == 0) return Array.Empty<Follow>();
            return await _dbSet
                   .Where(f => ids.Contains(f.FollowerId))
                   .ToListAsync(ct);
        }
        public async Task<int> GetFollowersCountForViewerAsync(
        Guid profileUserId,
        Guid viewerUserId,
        string? query,
        CancellationToken ct = default)
        {
            var blocks = _dbContext.Blocks;

            var q =
                from f in _dbSet
                join u in _dbContext.Users on f.FollowerId equals u.Id
                where f.FolloweeId == profileUserId
                      && !u.IsDeleted
                      // loại những user có block với viewer
                      && !blocks.Any(b =>
                            (b.BlockerId == viewerUserId && b.BlockedId == u.Id) ||
                            (b.BlockerId == u.Id && b.BlockedId == viewerUserId))
                select u;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var normalized = query.Trim().ToUpper();
                q = q.Where(u =>
                    u.NormalizedUserName.Contains(normalized) ||
                    u.DisplayName.ToUpper().Contains(normalized));
            }

            return await q.CountAsync(ct);
        }

        public async Task<IReadOnlyList<User>> GetFollowersForViewerAsync(
            Guid profileUserId,
            Guid viewerUserId,
            string? query,
            int skip,
            int take,
            CancellationToken ct = default)
        {
            var blocks = _dbContext.Blocks;

            var q =
                from f in _dbSet
                join u in _dbContext.Users on f.FollowerId equals u.Id
                where f.FolloweeId == profileUserId
                      && !u.IsDeleted
                      && !blocks.Any(b =>
                            (b.BlockerId == viewerUserId && b.BlockedId == u.Id) ||
                            (b.BlockerId == u.Id && b.BlockedId == viewerUserId))
                select u;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var normalized = query.Trim().ToUpper();
                q = q.Where(u =>
                    u.NormalizedUserName.Contains(normalized) ||
                    u.DisplayName.ToUpper().Contains(normalized));
            }

            return await q
                .OrderByDescending(u => u.CreatedAt) // hoặc theo Follow.CreatedAt nếu bạn muốn
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<int> GetFollowingCountForViewerAsync(
            Guid profileUserId,
            Guid viewerUserId,
            string? query,
            CancellationToken ct = default)
        {
            var blocks = _dbContext.Blocks;

            var q =
                from f in _dbSet
                join u in _dbContext.Users on f.FolloweeId equals u.Id
                where f.FollowerId == profileUserId
                      && !u.IsDeleted
                      && !blocks.Any(b =>
                            (b.BlockerId == viewerUserId && b.BlockedId == u.Id) ||
                            (b.BlockerId == u.Id && b.BlockedId == viewerUserId))
                select u;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var normalized = query.Trim().ToUpper();
                q = q.Where(u =>
                    u.NormalizedUserName.Contains(normalized) ||
                    u.DisplayName.ToUpper().Contains(normalized));
            }

            return await q.CountAsync(ct);
        }

        public async Task<IReadOnlyList<User>> GetFollowingForViewerAsync(
            Guid profileUserId,
            Guid viewerUserId,
            string? query,
            int skip,
            int take,
            CancellationToken ct = default)
        {
            var blocks = _dbContext.Blocks;

            var q =
                from f in _dbSet
                join u in _dbContext.Users on f.FolloweeId equals u.Id
                where f.FollowerId == profileUserId
                      && !u.IsDeleted
                      && !blocks.Any(b =>
                            (b.BlockerId == viewerUserId && b.BlockedId == u.Id) ||
                            (b.BlockerId == u.Id && b.BlockedId == viewerUserId))
                select u;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var normalized = query.Trim().ToUpper();
                q = q.Where(u =>
                    u.NormalizedUserName.Contains(normalized) ||
                    u.DisplayName.ToUpper().Contains(normalized));
            }

            return await q
                .OrderByDescending(u => u.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }
    }
}

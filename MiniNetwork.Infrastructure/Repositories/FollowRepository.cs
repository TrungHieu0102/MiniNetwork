using Microsoft.EntityFrameworkCore;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Infrastructure.Repositories
{
    public class FollowRepository :EfRepository<Follow> , IFollowRepository
    {
        public FollowRepository(MiniNetworkDbContext dbContext) : base(dbContext)
        {
            
        }

        public async Task<bool> ExistsAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId, cancellationToken);
        }

        public async Task<Follow?> GetAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId,
                cancellationToken);
        }

        public async Task<int> GetFollowersCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(f => f.FolloweeId == userId, cancellationToken);
        }

        public async Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(f => f.FollowerId == userId, cancellationToken);
        }
    }
}

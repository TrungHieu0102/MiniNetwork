using Microsoft.EntityFrameworkCore;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Domain.Enums;
using MiniNetwork.Infrastructure.Persistence;

namespace MiniNetwork.Infrastructure.Repositories;

public class UserTokenRepository : EfRepository<UserToken>, IUserTokenRepository
{
    public UserTokenRepository(MiniNetworkDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<UserToken?> GetActiveTokenAsync(
        string token,
        UserTokenType type,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                x => x.Token == token &&
                     x.Type == type &&
                     x.UsedAt == null &&
                     x.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    public Task AddTokenAsync(UserToken token, CancellationToken cancellationToken = default)
        => AddAsync(token, cancellationToken);
}

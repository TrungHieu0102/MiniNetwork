using Microsoft.EntityFrameworkCore;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Infrastructure.Persistence;

namespace MiniNetwork.Infrastructure.Repositories;

public class UserRepository : EfRepository<User>, IUserRepository
{
    public UserRepository(MiniNetworkDbContext dbContext) : base(dbContext)
    {
    }

    public Task<User?> GetByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        return _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public Task<bool> UserNameExistsAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        return _dbSet.AnyAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return _dbSet.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }
}

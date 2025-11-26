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
    public async Task<List<User>> SearchAsync(
       string? query,
       int take,
       CancellationToken ct = default)
    {
        var users = _dbSet
            .Where(u => !u.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();

            var qUpper = q.ToUpperInvariant();

            users = users.Where(u =>
                u.DisplayName.Contains(q) ||
                u.NormalizedUserName.Contains(qUpper) ||
                u.NormalizedEmail.Contains(qUpper));
        }

        return await users
            .OrderBy(u => u.DisplayName)
            .Take(take)
            .ToListAsync(ct);
    }
    public async Task<IReadOnlyList<User>> GetByIdsAsync(
            IReadOnlyList<Guid> ids,
            CancellationToken ct = default)
            {
                if (ids == null || ids.Count == 0)
                    return Array.Empty<User>();

                return await _dbSet
                    .Where(u => ids.Contains(u.Id))
                    .ToListAsync(ct);
            }

}

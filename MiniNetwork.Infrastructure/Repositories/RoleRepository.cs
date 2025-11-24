using Microsoft.EntityFrameworkCore;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Infrastructure.Persistence;

namespace MiniNetwork.Infrastructure.Repositories;

public class RoleRepository : EfRepository<Role>, IRoleRepository
{
    public RoleRepository(MiniNetworkDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Role?> GetByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, cancellationToken);
    }
}

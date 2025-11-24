using MiniNetwork.Domain.Entities;

namespace MiniNetwork.Application.Interfaces.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string normalizedName, CancellationToken cancellationToken = default);
}

using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Infrastructure.Persistence;

namespace MiniNetwork.Infrastructure.Uow;

public class UnitOfWork : IUnitOfWork
{
    private readonly MiniNetworkDbContext _dbContext;

    public UnitOfWork(MiniNetworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

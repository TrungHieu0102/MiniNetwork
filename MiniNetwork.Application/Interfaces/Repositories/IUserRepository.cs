using MiniNetwork.Domain.Entities;
using MiniNetwork.Domain.Enums;

namespace MiniNetwork.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<bool> UserNameExistsAsync(string normalizedUserName, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    
}

using MiniNetwork.Domain.Entities;
using MiniNetwork.Domain.Enums;

namespace MiniNetwork.Application.Interfaces.Repositories;

public interface IUserTokenRepository : IRepository<UserToken>
{
    Task<UserToken?> GetActiveTokenAsync(
        string token,
        UserTokenType type,
        CancellationToken cancellationToken = default);

    Task AddTokenAsync(UserToken token, CancellationToken cancellationToken = default);
}

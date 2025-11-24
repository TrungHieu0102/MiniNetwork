using MiniNetwork.Domain.Entities;

namespace MiniNetwork.Application.Interfaces.Services;

public interface IJwtTokenGenerator
{
    (string token, DateTime expiresAt) GenerateAccessToken(User user, IEnumerable<string> roles);
    (string token, DateTime expiresAt) GenerateRefreshToken(User user);
}

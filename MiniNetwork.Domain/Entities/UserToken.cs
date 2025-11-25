using MiniNetwork.Domain.Common;
using MiniNetwork.Domain.Enums;

namespace MiniNetwork.Domain.Entities;

public class UserToken : Entity
{
    private UserToken() { } // EF

    public UserToken(Guid userId, string token, UserTokenType type, DateTime expiresAt)
    {
        UserId = userId;
        Token = token;
        Type = type;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public UserTokenType Type { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public bool IsUsed => UsedAt.HasValue;
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActive => !IsUsed && !IsExpired;

    public void MarkUsed()
    {
        if (!IsUsed)
        {
            UsedAt = DateTime.UtcNow;
        }
    }

    // nav
    public User User { get; private set; } = null!;
}

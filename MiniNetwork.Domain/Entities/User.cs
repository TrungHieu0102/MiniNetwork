using MiniNetwork.Domain.Common;
using MiniNetwork.Domain.Enums;
using System.Xml.Linq;

namespace MiniNetwork.Domain.Entities;

public class User : SoftDeletableEntity
{
    public string UserName { get; private set; } = null!;
    public string NormalizedUserName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string NormalizedEmail { get; private set; } = null!;
    public bool EmailConfirmed { get; private set; }

    public string PasswordHash { get; private set; } = null!;
    public string? SecurityStamp { get; private set; }
    public string? ConcurrencyStamp { get; private set; }

    public string DisplayName { get; private set; } = null!;
    public string? Bio { get; private set; }
    public string? AvatarUrl { get; private set; }

    public UserStatus Status { get; private set; } = UserStatus.Active;

    // Navigation
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<Post> Posts { get; private set; } = new List<Post>();
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; private set; } = new List<PostLike>();
    public ICollection<Follow> Followers { get; private set; } = new List<Follow>();
    public ICollection<Follow> Following { get; private set; } = new List<Follow>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User() { }

    public User(string userName, string email, string passwordHash, string displayName)
    {
        SetUserName(userName);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        DisplayName = displayName;
        SecurityStamp = Guid.NewGuid().ToString("N");
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public void SetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("UserName is required.", nameof(userName));

        UserName = userName.Trim();
        NormalizedUserName = UserName.ToUpperInvariant();
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        Email = email.Trim();
        NormalizedEmail = Email.ToUpperInvariant();
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        PasswordHash = passwordHash;
        SecurityStamp = Guid.NewGuid().ToString("N"); // đổi password => đổi security stamp
    }

    public void UpdateProfile(string displayName, string? bio, string? avatarUrl)
    {
        DisplayName = displayName;
        Bio = bio;
        AvatarUrl = avatarUrl;
        MarkUpdated(Id);
    }

    public void SetStatus(UserStatus status)
    {
        Status = status;
        MarkUpdated(Id);
    }
}

namespace MiniNetwork.Application.Auth.DTOs;

public class AuthResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string[] Roles { get; set; } = Array.Empty<string>();

    public string AccessToken { get; set; } = null!;
    public DateTime AccessTokenExpiresAt { get; set; }

    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiresAt { get; set; }
}

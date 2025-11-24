namespace MiniNetwork.Application.Auth.DTOs;

public class MeResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}

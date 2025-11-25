namespace MiniNetwork.Application.Users.DTOs;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}

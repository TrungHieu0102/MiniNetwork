namespace MiniNetwork.Application.Users.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string Status { get; set; } = null!; // map từ enum Status.ToString()
}

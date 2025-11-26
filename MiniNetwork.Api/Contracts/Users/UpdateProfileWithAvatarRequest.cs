using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MiniNetwork.Api.Contracts.Users;

public class UpdateProfileWithAvatarRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string DisplayName { get; set; } = null!;

    [StringLength(500)]
    public string? Bio { get; set; }

    public IFormFile? Avatar { get; set; }
}

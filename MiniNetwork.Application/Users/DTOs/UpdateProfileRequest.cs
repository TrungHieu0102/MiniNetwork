using System.ComponentModel.DataAnnotations;

namespace MiniNetwork.Application.Users.DTOs;

public class UpdateProfileRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "DisplayName phải từ 2 đến 50 ký tự.")]
    public string DisplayName { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Bio tối đa 500 ký tự.")]
    public string? Bio { get; set; }

    [Url(ErrorMessage = "AvatarUrl phải là URL hợp lệ.")]
    [StringLength(500)]
    public string? AvatarUrl { get; set; }
}

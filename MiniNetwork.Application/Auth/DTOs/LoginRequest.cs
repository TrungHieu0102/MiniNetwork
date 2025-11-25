using System.ComponentModel.DataAnnotations;

namespace MiniNetwork.Application.Auth.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Vui lòng nhập username hoặc email.")]
    [StringLength(256, ErrorMessage = "Username/Email quá dài.")]
    public string UserNameOrEmail { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
    public string Password { get; set; } = null!;
}

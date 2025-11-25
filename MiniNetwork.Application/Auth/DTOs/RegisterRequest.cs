using System.ComponentModel.DataAnnotations;

namespace MiniNetwork.Application.Auth.DTOs;

public class RegisterRequest
{
    [Required]
    [StringLength(32, MinimumLength = 3,
        ErrorMessage = "UserName phải từ 3 đến 32 ký tự.")]
    [RegularExpression("^[a-zA-Z0-9_]+$",
        ErrorMessage = "UserName chỉ được chứa chữ cái, số và dấu gạch dưới.")]
    public string UserName { get; set; } = null!;

    [Required]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(256, ErrorMessage = "Email quá dài.")]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
    // optional: ít nhất 1 chữ hoa, 1 chữ thường, 1 số
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$",
        ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ thường, 1 chữ hoa và 1 số.")]
    public string Password { get; set; } = null!;

    [Required]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Tên hiển thị phải từ 2 đến 50 ký tự.")]
    public string DisplayName { get; set; } = null!;
}

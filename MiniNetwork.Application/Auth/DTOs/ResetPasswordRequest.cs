using System.ComponentModel.DataAnnotations;

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = null!;
}

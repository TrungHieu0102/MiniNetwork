using System.ComponentModel.DataAnnotations;

namespace MiniNetwork.Application.Auth.DTOs;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniNetwork.Application.Auth;
using MiniNetwork.Application.Auth.DTOs;
using System.IdentityModel.Tokens.Jwt;

namespace MiniNetwork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        if (!result.Succeeded) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request, ip, ct);
        if (!result.Succeeded) return Unauthorized(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshTokenAsync(request, ip, ct);
        if (!result.Succeeded) return Unauthorized(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LogoutAsync(request.RefreshToken, ip, ct);
        if (!result.Succeeded) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized();

        request.UserId = userId;
        var result = await _authService.ChangePasswordAsync(request, ct);
        if (!result.Succeeded) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _authService.GetCurrentUserAsync(userId, ct);
        if (!result.Succeeded || result.Data == null)
            return NotFound(new { error = result.Error });

        return Ok(result.Data);
    }
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(
    ForgotPasswordRequest request,
    CancellationToken ct)
    {
        var result = await _authService.ForgotPasswordAsync(request, ct);
        // luôn trả 200 để tránh lộ email có tồn tại hay không
        return Ok();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken ct)
    {
        var result = await _authService.ResetPasswordAsync(request, ct);
        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    private Guid GetUserIdFromClaims()
    {
        // thử theo kiểu chuẩn trước
        var userIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier) // nếu middleware map "sub" -> NameIdentifier
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub) // đọc trực tiếp claim "sub"
            ?? User.FindFirstValue("sub");                      // fallback

        return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
    }
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWithGoogle(GoogleLoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginWithGoogleAsync(request, ip, ct);

        if (!result.Succeeded)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Data);
    }
}

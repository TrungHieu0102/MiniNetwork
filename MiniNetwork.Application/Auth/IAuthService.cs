using MiniNetwork.Application.Auth.DTOs;
using MiniNetwork.Application.Common;

namespace MiniNetwork.Application.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<Result<MeResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> ForgotPasswordAsync(
    ForgotPasswordRequest request,
    CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);
}

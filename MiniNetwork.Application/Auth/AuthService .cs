using MiniNetwork.Application.Auth.DTOs;
using MiniNetwork.Application.Common;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Application.Interfaces.Services;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Domain.Enums;

namespace MiniNetwork.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateService _emailTemplateService;



    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
         IUserTokenRepository userTokenRepository,
         IEmailSender emailSender,
         IEmailTemplateService emailTemplateService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _userTokenRepository = userTokenRepository;
        _emailSender = emailSender;
        _emailTemplateService = emailTemplateService;
    }

    // ========== REGISTER ==========

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedUserName = request.UserName.Trim().ToUpperInvariant();
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        if (await _userRepository.UserNameExistsAsync(normalizedUserName, cancellationToken))
        {
            return Result<AuthResponse>.Failure("Username already taken.");
        }

        if (await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return Result<AuthResponse>.Failure("Email already in use.");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User(
            userName: request.UserName,
            email: request.Email,
            passwordHash: passwordHash,
            displayName: request.DisplayName
        );

        // Gán role mặc định "User"
        const string defaultRoleName = "User";
        var normalizedRoleName = defaultRoleName.ToUpperInvariant();

        var role = await _roleRepository.GetByNameAsync(normalizedRoleName, cancellationToken);

        if (role is null)
        {
            role = new Role(defaultRoleName, "Default application user");
            await _roleRepository.AddAsync(role, cancellationToken);
        }

        user.UserRoles.Add(new UserRole(user.Id, role.Id));

        await _userRepository.AddAsync(user, cancellationToken);

        // Tạo token luôn sau khi đăng ký
        var roles = new[] { defaultRoleName };
        var (accessToken, accessExpires) = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var (refreshTokenValue, refreshExpires) = _jwtTokenGenerator.GenerateRefreshToken(user);

        var refreshToken = new RefreshToken(user.Id, refreshTokenValue, refreshExpires, null);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Roles = roles,
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpires,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresAt = refreshExpires
        };

        return Result<AuthResponse>.Success(response);
    }

    // ========== LOGIN ==========

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var identifier = request.UserNameOrEmail.Trim();
        var normalized = identifier.ToUpperInvariant();

        User? user =
            await _userRepository.GetByUserNameAsync(normalized, cancellationToken)
            ?? await _userRepository.GetByEmailAsync(normalized, cancellationToken);

        if (user is null)
        {
            return Result<AuthResponse>.Failure("Invalid credentials.");
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<AuthResponse>.Failure("User is not active.");
        }

        if (!_passwordHasher.VerifyHashedPassword(user.PasswordHash, request.Password))
        {
            return Result<AuthResponse>.Failure("Wrong password");
        }

        var roles = user.UserRoles
            .Select(ur => ur.Role.Name)
            .ToArray();

        var (accessToken, accessExpires) = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var (refreshTokenValue, refreshExpires) = _jwtTokenGenerator.GenerateRefreshToken(user);

        var refreshToken = new RefreshToken(user.Id, refreshTokenValue, refreshExpires, ipAddress);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        // 🔴 LƯU VIỆC THÊM REFRESH TOKEN
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Roles = roles,
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpires,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresAt = refreshExpires
        };

        return Result<AuthResponse>.Success(response);
    }

    // ========== REFRESH TOKEN ==========

    public async Task<Result<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var existingToken =
            await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return Result<AuthResponse>.Failure("Invalid or inactive refresh token.");
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);

        if (user is null || user.Status != UserStatus.Active)
        {
            return Result<AuthResponse>.Failure("User is not active.");
        }

        var roles = user.UserRoles
            .Select(ur => ur.Role.Name)
            .ToArray();

        var (accessToken, accessExpires) = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var (newRefreshTokenValue, newRefreshExpires) = _jwtTokenGenerator.GenerateRefreshToken(user);

        // Revoke token cũ
        existingToken.Revoke(ipAddress, newRefreshTokenValue);
        _refreshTokenRepository.Update(existingToken);

        // Lưu token mới
        var newRefreshToken = new RefreshToken(user.Id, newRefreshTokenValue, newRefreshExpires, ipAddress);
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        // 🔴 LƯU CẢ HAI THAY ĐỔI TRONG 1 TRANSACTION
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Roles = roles,
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpires,
            RefreshToken = newRefreshTokenValue,
            RefreshTokenExpiresAt = newRefreshExpires
        };

        return Result<AuthResponse>.Success(response);
    }

    // ========== LOGOUT ==========

    public async Task<Result> LogoutAsync(
        string refreshToken,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

        if (token is null || !token.IsActive)
        {
            // Vẫn coi là success để tránh lộ thông tin
            return Result.Success();
        }

        token.Revoke(ipAddress);
        _refreshTokenRepository.Update(token);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    // ========== CHANGE PASSWORD ==========

    public async Task<Result> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        if (!_passwordHasher.VerifyHashedPassword(user.PasswordHash, request.CurrentPassword))
        {
            return Result.Failure("Current password is incorrect.");
        }

        var newHash = _passwordHasher.HashPassword(request.NewPassword);
        user.SetPasswordHash(newHash);

        _userRepository.Update(user);

        // (tuỳ bạn) có thể revoke all refresh token ở đây sau này
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    // ========== GET CURRENT USER (/me) ==========

    public async Task<Result<MeResponse>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<MeResponse>.Failure("User not found.");
        }

        var roles = user.UserRoles
            .Select(ur => ur.Role.Name)
            .ToArray();

        var me = new MeResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Roles = roles
        };

        return Result<MeResponse>.Success(me);
    }
    public async Task<Result> ForgotPasswordAsync(
    ForgotPasswordRequest request,
    CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        // Không lộ user tồn tại hay không
        if (user is null)
        {
            return Result.Success();
        }

        // Tạo token reset
        var tokenValue = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var token = new UserToken(
            userId: user.Id,
            token: tokenValue,
            type: UserTokenType.PasswordReset,
            expiresAt: expiresAt);

        await _userTokenRepository.AddTokenAsync(token, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 🔴 Link local tạm thời
        var resetLink = $"https://localhost:5173/reset-password?token={Uri.EscapeDataString(tokenValue)}";

        var html = _emailTemplateService.Render("ResetPasswordTemplate.html", new()
        {
            ["DisplayName"] = user.DisplayName,
            ["ResetLink"] = resetLink,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        });

        await _emailSender.SendEmailAsync(user.Email, "Reset password", html, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(
    ResetPasswordRequest request,
    CancellationToken cancellationToken = default)
    {
        var rawToken = Uri.UnescapeDataString(request.Token);

        var token = await _userTokenRepository.GetActiveTokenAsync(
          rawToken,
          UserTokenType.PasswordReset,
          cancellationToken);

        if (token is null)
        {
            return Result.Failure("Token không hợp lệ hoặc đã hết hạn.");
        }

        var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("User không tồn tại.");
        }

        // Đổi mật khẩu
        var newHash = _passwordHasher.HashPassword(request.NewPassword);
        user.SetPasswordHash(newHash);

        // Đánh dấu token đã dùng
        token.MarkUsed();

        _userRepository.Update(user);
        _userTokenRepository.Update(token);

        // (optional) revoke tất cả refresh token cũ của user
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

}

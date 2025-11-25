using MiniNetwork.Application.Common;
using MiniNetwork.Application.Users.DTOs;

namespace MiniNetwork.Application.Users;

public interface IUserService
{
    Task<Result<UserProfileDto>> GetUserProfileAsync(Guid id, CancellationToken ct);
    Task<Result<UserProfileDto>> GetCurrentUserProfileAsync(Guid id, CancellationToken ct);
    Task<Result> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct);
    Task<Result<List<UserSummaryDto>>> SearchUsersAsync(string? query, CancellationToken ct);
}

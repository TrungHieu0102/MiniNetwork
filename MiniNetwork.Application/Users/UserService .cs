using AutoMapper;
using MiniNetwork.Application.Common;
using MiniNetwork.Application.Follows;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Application.Users.DTOs;
using MiniNetwork.Domain.Entities;

namespace MiniNetwork.Application.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFollowService _followService;
    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper, IFollowService followService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _followService = followService;
    }

    public async Task<Result<UserProfileDto>> GetCurrentUserProfileAsync(
        Guid userId,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null || user.IsDeleted)
            return Result<UserProfileDto>.Failure("User not found.");

        var dto = _mapper.Map<UserProfileDto>(user);
        var followersCount = await _followService.GetFollowersCountAsync(userId, ct);
        var followingCount = await _followService.GetFollowingCountAsync(userId, ct);
        dto.FollowersCount = followersCount.Data;
        dto.FollowingCount = followingCount.Data;
        return Result<UserProfileDto>.Success(dto);
    }

    public async Task<Result<UserProfileDto>> GetUserProfileAsync(
         Guid profileUserId,
    Guid currentUserId,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(profileUserId, ct);

        if (user is null || user.IsDeleted)
            return Result<UserProfileDto>.Failure("User not found.");

        var dto = _mapper.Map<UserProfileDto>(user);
        if (currentUserId != Guid.Empty && currentUserId != profileUserId)
        {
            var followResult = await _followService.IsFollowingAsync(currentUserId, profileUserId, ct);
            dto.IsFollowing = followResult.Data;
        }
        else
        {
            dto.IsFollowing = false;
        }
        var followersCount = await _followService.GetFollowersCountAsync(profileUserId, ct);
        var followingCount = await _followService.GetFollowingCountAsync(profileUserId, ct);

        dto.FollowersCount = followersCount.Data;
        dto.FollowingCount = followingCount.Data;

        return Result<UserProfileDto>.Success(dto);
    }

    public async Task<Result> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);

        if (user is null || user.IsDeleted)
            return Result.Failure("User không tồn tại.");

        user.UpdateProfile(
            request.DisplayName,
            request.Bio,
            request.AvatarUrl
        );

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<List<UserSummaryDto>>> SearchUsersAsync(
     string? query,
     CancellationToken ct)
    {
        var users = await _userRepository.SearchAsync(query, 20, ct);

        var dto = _mapper.Map<List<UserSummaryDto>>(users);

        return Result<List<UserSummaryDto>>.Success(dto);
    }
    public async Task<Result<string>> UpdateAvatarAsync(
    Guid userId,
    string avatarUrl,
    CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null || user.IsDeleted)
            return Result<string>.Failure("User không tồn tại.");

        user.UpdateProfile(user.DisplayName, user.Bio, avatarUrl);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<string>.Success(avatarUrl);
    }


}

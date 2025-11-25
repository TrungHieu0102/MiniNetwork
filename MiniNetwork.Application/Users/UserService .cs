using AutoMapper;
using MiniNetwork.Application.Common;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Application.Users.DTOs;
using MiniNetwork.Domain.Entities;

namespace MiniNetwork.Application.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserProfileDto>> GetCurrentUserProfileAsync(
        Guid userId,
        CancellationToken ct)
    {
        return await GetUserProfileAsync(userId, ct);
    }

    public async Task<Result<UserProfileDto>> GetUserProfileAsync(
        Guid userId,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);

        if (user is null || user.IsDeleted)
            return Result<UserProfileDto>.Failure("User không tồn tại.");

        var dto = _mapper.Map<UserProfileDto>(user);
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

}

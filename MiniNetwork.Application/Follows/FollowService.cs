using MiniNetwork.Application.Common;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;

namespace MiniNetwork.Application.Follows
{
    public class FollowService(IFollowRepository followRepository, IUserRepository userRepository, IUnitOfWork unitOfWork) : IFollowService
    {
        private readonly IFollowRepository _followRepository = followRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result> FollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken ct)
        {
            //double check business rules
            if (currentUserId == targetUserId)
            {
                return Result.Failure("You cannot follow yourself.");
            }
            var targetUser = await _userRepository.GetByIdAsync(targetUserId, ct);
            if (targetUser == null || targetUser.IsDeleted)
            {
                return Result.Failure("The user you are trying to follow does not exist.");
            }
            if (targetUser.Status != Domain.Enums.UserStatus.Active)
            {
                return Result.Failure("You can only follow active users.");
            }
            var existingFollow = await _followRepository.ExistsAsync(currentUserId, targetUserId, ct);
            if (existingFollow)
            {
                return Result.Failure("You are already following this user.");
            }
            var follow = new Follow(
               currentUserId,
               targetUserId
            );
            await _followRepository.AddAsync(follow, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> UnfollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken ct)
        {
            if (currentUserId == targetUserId)
            {
                return Result.Failure("You cannot unfollow yourself.");
            }
            var existingFollow = await _followRepository.GetAsync(currentUserId, targetUserId, ct);
            if (existingFollow == null)
            {
                return Result.Failure("You are not following this user.");
            }
            _followRepository.Remove(existingFollow);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result<bool>> IsFollowingAsync(Guid currentUserId, Guid targetUserId, CancellationToken ct)
        {
            if (currentUserId == targetUserId || currentUserId == Guid.Empty || targetUserId == Guid.Empty)
            {
                return Result<bool>.Success(false);
            }
            var isFollowing = await _followRepository.ExistsAsync(currentUserId, targetUserId, ct);
            return Result<bool>.Success(isFollowing);
        }

        public async Task<Result<int>> GetFollowersCountAsync(Guid userId, CancellationToken ct)
        {
            if (userId == Guid.Empty)
            {
                return Result<int>.Failure("Invalid user ID.");
            }
            var count = await _followRepository.GetFollowersCountAsync(userId, ct);
            return Result<int>.Success(count);
        }

        public async Task<Result<int>> GetFollowingCountAsync(Guid userId, CancellationToken ct)
        {
            if (userId == Guid.Empty)
            {
                return Result<int>.Failure("Invalid user ID.");
            }
            var count = await _followRepository.GetFollowingCountAsync(userId, ct);
            return Result<int>.Success(count);
        }
    }
}

using AutoMapper;
using MiniNetwork.Application.Common;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Application.Users.DTOs;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Domain.Enums;

namespace MiniNetwork.Application.Follows
{
    public class FollowService(IFollowRepository followRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IMapper mapper) : IFollowService
    {
        private readonly IFollowRepository _followRepository = followRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

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

        public async Task<Result<PagedResult<UserSummaryDto>>> GetFollowersAsync(
                Guid userId,
                string? query,
                int page,
                int pageSize,
                CancellationToken ct)
        {
            if (userId == Guid.Empty)
                return Result<PagedResult<UserSummaryDto>>.Failure("UserId không hợp lệ.");

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            var skip = (page - 1) * pageSize;

            var totalCount = await _followRepository.GetFollowersCountAsync(userId, ct);
            var users = await _followRepository.GetFollowersAsync(userId, query, skip, pageSize, ct);

            var dto = _mapper.Map<List<UserSummaryDto>>(users);
            var paged = PagedResult<UserSummaryDto>.Create(dto, page, pageSize, totalCount);

            return Result<PagedResult<UserSummaryDto>>.Success(paged);
        }

        public async Task<Result<PagedResult<UserSummaryDto>>> GetFollowingAsync(
                 Guid userId,
                 string? query,
                 int page,
                 int pageSize,
                 CancellationToken ct)
        {
            if (userId == Guid.Empty)
                return Result<PagedResult<UserSummaryDto>>.Failure("UserId không hợp lệ.");

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            var skip = (page - 1) * pageSize;

            var totalCount = await _followRepository.GetFollowingCountAsync(userId, ct);
            var users = await _followRepository.GetFollowingAsync(userId, query, skip, pageSize, ct);

            var dto = _mapper.Map<List<UserSummaryDto>>(users);
            var paged = PagedResult<UserSummaryDto>.Create(dto, page, pageSize, totalCount);

            return Result<PagedResult<UserSummaryDto>>.Success(paged);
        }

        public async Task<Result<List<UserSummaryDto>>> SuggestFollowsAsync(Guid currentUserId, int limit, CancellationToken ct)
        {
            if (currentUserId == Guid.Empty)
            {
                return Result<List<UserSummaryDto>>.Failure("Invalid user ID.");
            }
            if (limit <= 0 || limit > 50)
            {
                limit = 10;
            }
            var followingIds = await _followRepository.GetFollowingIdsAsync(currentUserId, ct);
            var alreadyFollowing = new HashSet<Guid>(followingIds);
            alreadyFollowing.Add(currentUserId);
            if (followingIds.Count == 0)
            {
                // Nếu chưa follow ai, có thể gợi ý theo tiêu chí khác (top user, mới tạo, v.v.)
                // Ở đây trả list rỗng cho đơn giản
                // Sau này implement thêm logic gợi ý
                return Result<List<UserSummaryDto>>.Success(new List<UserSummaryDto>());
            }
            var friendsOfFriendsEdges = await _followRepository.GetFollowingsByFollowerIdsAsync(followingIds, ct);
            var scores = new Dictionary<Guid, int>();
            foreach (var edge in friendsOfFriendsEdges)
            {
                var candidateId = edge.FolloweeId;

                // Bỏ qua mình + những người mình đã follow
                if (alreadyFollowing.Contains(candidateId))
                    continue;

                if (scores.ContainsKey(candidateId))
                    scores[candidateId]++;
                else
                    scores[candidateId] = 1;
            }
            if (scores.Count == 0)
            {
                return Result<List<UserSummaryDto>>.Success(new List<UserSummaryDto>());
            }
            //Take top candidates
            var topCandidateIds = scores
                   .OrderByDescending(kv => kv.Value)
                   .ThenBy(kv => kv.Key)
                   .Take(limit)
                   .Select(kv => kv.Key)
                   .ToList();
            var users = await _userRepository.GetByIdsAsync(topCandidateIds, ct);
            var userDict = users.ToDictionary(u => u.Id);
            var orderedUsers = topCandidateIds
                    .Where(id => userDict.ContainsKey(id))
                    .Select(id => userDict[id])
                    .ToList();
            orderedUsers = [.. orderedUsers.Where(u => !u.IsDeleted && u.Status == UserStatus.Active)];

            var dto = _mapper.Map<List<UserSummaryDto>>(orderedUsers);

            return Result<List<UserSummaryDto>>.Success(dto);
        }
    }
}

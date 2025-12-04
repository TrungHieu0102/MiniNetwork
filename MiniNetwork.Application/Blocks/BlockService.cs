using MiniNetwork.Application.Common;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Domain.Enums;

namespace MiniNetwork.Application.Blocks;

public class BlockService : IBlockService
{
    private readonly IBlockRepository _blockRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFollowRepository _followRepository;
    private readonly IUnitOfWork _unitOfWork;
    public BlockService(
        IBlockRepository blockRepository,
        IUserRepository userRepository,
        IFollowRepository followRepository,
        IUnitOfWork unitOfWork)
    {
        _blockRepository = blockRepository;
        _userRepository = userRepository;
        _followRepository = followRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> BlockAsync(
        Guid currentUserId,
        Guid targetUserId,
        CancellationToken ct)
    {
        if (currentUserId == Guid.Empty || targetUserId == Guid.Empty)
            return Result.Failure("UserId không hợp lệ.");

        if (currentUserId == targetUserId)
            return Result.Failure("Bạn không thể block chính mình.");

        var targetUser = await _userRepository.GetByIdAsync(targetUserId, ct);
        if (targetUser is null || targetUser.IsDeleted)
            return Result.Failure("User không tồn tại.");

        // Đã block rồi thì thôi
        var exists = await _blockRepository.ExistsAsync(currentUserId, targetUserId, ct);
        if (exists)
            return Result.Success(); // idempotent

        // Tạo block
        var block = new Block(currentUserId, targetUserId);
        await _blockRepository.AddAsync(block, ct);

        // Optional: huỷ follow cả 2 chiều
        var follow1 = await _followRepository.GetAsync(currentUserId, targetUserId, ct);
        if (follow1 is not null)
            _followRepository.Remove(follow1);

        var follow2 = await _followRepository.GetAsync(targetUserId, currentUserId, ct);
        if (follow2 is not null)
            _followRepository.Remove(follow2);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> UnblockAsync(
        Guid currentUserId,
        Guid targetUserId,
        CancellationToken ct)
    {
        if (currentUserId == Guid.Empty || targetUserId == Guid.Empty)
            return Result.Failure("UserId không hợp lệ.");

        if (currentUserId == targetUserId)
            return Result.Failure("Bạn không thể unblock chính mình.");

        var existing = await _blockRepository.GetAsync(currentUserId, targetUserId, ct);
        if (existing is null)
            return Result.Success(); // idempotent

        _blockRepository.Remove(existing);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<bool>> IsBlockedBetweenAsync(
        Guid userAId,
        Guid userBId,
        CancellationToken ct)
    {
        if (userAId == Guid.Empty || userBId == Guid.Empty || userAId == userBId)
            return Result<bool>.Success(false);

        var blocked = await _blockRepository.IsBlockedBetweenAsync(userAId, userBId, ct);
        return Result<bool>.Success(blocked);
    }
}

using MiniNetwork.Application.Common;

namespace MiniNetwork.Application.Blocks;

public interface IBlockService
{
    Task<Result> BlockAsync(
        Guid currentUserId,
        Guid targetUserId,
        CancellationToken ct);

    Task<Result> UnblockAsync(
        Guid currentUserId,
        Guid targetUserId,
        CancellationToken ct);

    Task<Result<bool>> IsBlockedBetweenAsync(
        Guid userAId,
        Guid userBId,
        CancellationToken ct);
}

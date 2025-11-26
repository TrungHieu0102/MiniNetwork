using MiniNetwork.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Follows
{
    public interface IFollowService
    {
        Task<Result> FollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken ct);
        Task<Result> UnfollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken ct);
        Task<Result<bool>> IsFollowingAsync(Guid currentUserId, Guid targetUserId, CancellationToken ct);
        Task<Result<int>> GetFollowersCountAsync(
           Guid userId,
           CancellationToken ct);

        Task<Result<int>> GetFollowingCountAsync(
            Guid userId,
            CancellationToken ct);
    }
}

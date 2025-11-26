using MiniNetwork.Application.Common;
using MiniNetwork.Application.Users.DTOs;
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
        Task<Result<PagedResult<UserSummaryDto>>> GetFollowersAsync(
            Guid profileUserId,
            Guid viewerUserId,
            string? query,
            int page,
            int pageSize,
            CancellationToken ct);


        Task<Result<PagedResult<UserSummaryDto>>> GetFollowingAsync(
                Guid profileUserId,
                Guid viewerUserId,
                string? query,
                int page,
                int pageSize,
                CancellationToken ct);
        Task<Result<List<UserSummaryDto>>> SuggestFollowsAsync(
                  Guid currentUserId,
                  int limit,
                  CancellationToken ct);
        Task<Result<List<UserSummaryDto>>> SuggestFollowsRandomWalkAsync(Guid currentUserId, int limit, CancellationToken ct);
    }
}

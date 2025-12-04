using MiniNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Interfaces.Repositories
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<Post?> GetDetailAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Post>> GetPostsByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
        Task<bool> HasUserLikedAsync(Guid postId, Guid userId, CancellationToken ct = default);
        Task AddLikeAsync(Guid postId, Guid userId, CancellationToken ct = default);
        Task RemoveLikeAsync(Guid postId, Guid userId, CancellationToken ct = default);
    }
}

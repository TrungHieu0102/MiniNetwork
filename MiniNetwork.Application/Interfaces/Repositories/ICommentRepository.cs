using MiniNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Interfaces.Repositories
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<List<Comment>> GetPostCommentsAsync(Guid postId, int page, int pageSize, CancellationToken ct = default);
        Task<Comment?> GetByIdIncludeAsync(Guid id, CancellationToken ct = default);
    }
}

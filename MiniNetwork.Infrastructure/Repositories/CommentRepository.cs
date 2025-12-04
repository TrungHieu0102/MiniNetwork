using Microsoft.EntityFrameworkCore;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Infrastructure.Persistence;

namespace MiniNetwork.Infrastructure.Repositories
{
    internal class CommentRepository : EfRepository<Comment>, ICommentRepository
    {
        public CommentRepository(MiniNetworkDbContext dbContext) : base(dbContext) { }

        public async Task<Comment?> GetByIdIncludeAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContext.Comments
                          .Include(c => c.Author)
                          .Include(c => c.Replies)
                            .ThenInclude(r => r.Author)
                         .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<List<Comment>> GetPostCommentsAsync(Guid postId, int page, int pageSize, CancellationToken ct = default)
        {
            return await _dbContext.Comments
                       .Include(c => c.Author)
                       .Include(c => c.Replies)
                           .ThenInclude(r => r.Author)
                       .Where(c => c.PostId == postId && c.ParentCommentId == null)
                       .OrderByDescending(c => c.CreatedAt)   
                       .Skip((page - 1) * pageSize)
                       .Take(pageSize)
                       .ToListAsync(ct);
        }
    }
}

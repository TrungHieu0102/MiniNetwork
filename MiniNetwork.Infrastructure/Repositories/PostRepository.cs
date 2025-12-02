using Microsoft.EntityFrameworkCore;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Infrastructure.Persistence;

namespace MiniNetwork.Infrastructure.Repositories;

public class PostRepository(MiniNetworkDbContext dbContext) : EfRepository<Post>(dbContext), IPostRepository
{
    public async Task<Post?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.Images)
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Post>> GetPostsByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Images)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .ToListAsync(ct);
    }
}

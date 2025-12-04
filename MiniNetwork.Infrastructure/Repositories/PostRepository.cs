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
              .ThenInclude(l => l.User)
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
    public async Task<bool> HasUserLikedAsync(Guid postId, Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.PostLikes
            .AnyAsync(l => l.PostId == postId && l.UserId == userId, ct);
    }

    public async Task AddLikeAsync(Guid postId, Guid userId, CancellationToken ct = default)
    {
        var alreadyLiked = await HasUserLikedAsync(postId, userId, ct);
        if (alreadyLiked) return;

        var like = new PostLike(postId, userId); 
        _dbContext.PostLikes.Add(like);
    }

    public async Task RemoveLikeAsync(Guid postId, Guid userId, CancellationToken ct = default)
    {
        var like = await _dbContext.PostLikes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId, ct);

        if (like is null) return;

        _dbContext.PostLikes.Remove(like);
    }


}

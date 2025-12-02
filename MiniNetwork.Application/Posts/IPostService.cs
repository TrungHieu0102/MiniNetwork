using MiniNetwork.Application.Common;
using MiniNetwork.Application.Posts.DTOs;
using MiniNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Posts
{
    public interface IPostService
    {
        Task<Result<PostDto>> CreateAsync(Guid userId, string content, IEnumerable<Stream>? images, CancellationToken ct = default);

        Task<Result<PostDto>> GetByIdAsync(Guid postId, CancellationToken ct = default);

        Task<Result> UpdateContentAsync(Guid postId, Guid userId, string content, CancellationToken ct = default);

        Task<Result> AddImagesAsync(Guid postId, Guid userId, IEnumerable<Stream> images, CancellationToken ct = default);

        Task<Result> DeleteImageAsync(Guid postId, Guid userId, Guid imageId, CancellationToken ct = default);

        Task<Result> DeleteAsync(Guid postId, Guid userId, CancellationToken ct = default);
        Task<Result<IReadOnlyList<PostDto>>> GetUserPostsAsync( Guid authorId, int page, int pageSize,CancellationToken ct = default);
    }
}

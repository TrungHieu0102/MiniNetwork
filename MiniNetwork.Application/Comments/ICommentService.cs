using MiniNetwork.Application.Comments.DTOs;
using MiniNetwork.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Comments
{
    public interface ICommentService
    {
        Task<Result<CommentDto>> AddCommentAsync(Guid postId, Guid userId, string content, CancellationToken ct = default);
        Task<Result<CommentDto>> ReplyToCommentAsync(Guid parentCommentId, Guid userId, string content, CancellationToken ct = default);
        Task<Result> DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken ct = default);
        Task<Result<IReadOnlyList<CommentDto>>> GetPostCommentsAsync(Guid postId, int page, int pageSize, CancellationToken ct = default);
    }
}

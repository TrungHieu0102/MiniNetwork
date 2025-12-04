using MiniNetwork.Application.Comments.DTOs;
using MiniNetwork.Application.Common;
using MiniNetwork.Application.Interfaces.Repositories;
using MiniNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Comments
{
    public class CommentService : ICommentService
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IBlockRepository _blockRepository;
        private readonly IUnitOfWork _unitOfWork;
        public CommentService(
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IBlockRepository blockRepository,
            IUnitOfWork unitOfWork)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _blockRepository = blockRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CommentDto>> AddCommentAsync(Guid postId, Guid userId, string content, CancellationToken ct = default)
        {
            var post = await _postRepository.GetByIdAsync(postId, ct);
            if (post == null)
            {
                return Result<CommentDto>.Failure("Post not found.");
            }
            bool isBlocked = await _blockRepository.IsBlockedBetweenAsync(post.AuthorId, userId, ct);
            if (isBlocked)
            {
                return Result<CommentDto>.Failure("You cannot interact with this user.");
            }
            if (post.Visibility == Domain.Enums.PostVisibility.Private && post.AuthorId != userId)
            {
                return Result<CommentDto>.Failure("You cannot comment on this post.");
            }
            var comment = new Comment(postId, userId, content, null);
            await _commentRepository.AddAsync(comment, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            comment = await _commentRepository.GetByIdIncludeAsync(comment.Id, ct) ?? comment;
            var dto = MapToDto(comment);
            return Result<CommentDto>.Success(dto);
        }

        public async Task<Result> DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken ct = default)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId, ct);
            if (comment == null)
            {
                return Result.Failure("Comment not found.");
            }
            if (comment.AuthorId != userId)
            {
                return Result.Failure("You are not authorized to delete this comment.");
            }
            comment.SoftDelete(userId);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result<IReadOnlyList<CommentDto>>> GetPostCommentsAsync(Guid postId, int page, int pageSize, CancellationToken ct = default)
        {
            var post = await _postRepository.GetByIdAsync(postId, ct);
            if (post == null)
            {
                return Result<IReadOnlyList<CommentDto>>.Failure("Post not found.");
            }
            var comments = await _commentRepository.GetPostCommentsAsync(postId, page, pageSize, ct);
            var dtos = comments
              .Select(MapToTreeDto)
              .ToList()
              .AsReadOnly();
            return Result<IReadOnlyList<CommentDto>>.Success(dtos);
        }

        public async Task<Result<CommentDto>> ReplyToCommentAsync(Guid parentCommentId, Guid userId, string content, CancellationToken ct = default)
        {
            var parentComment = await _commentRepository.GetByIdIncludeAsync(parentCommentId, ct);
            if (parentComment == null)
            {
                return Result<CommentDto>.Failure("Parent comment not found.");
            }
            var post = await _postRepository.GetByIdAsync(parentComment.PostId, ct);
            if (post == null)
            {
                return Result<CommentDto>.Failure("Post not found.");
            }
            bool isBlocked = await _blockRepository.IsBlockedBetweenAsync(post.AuthorId, userId, ct);
            if (isBlocked)
            {
                return Result<CommentDto>.Failure("You cannot interact with this user.");
            }
            if (post.Visibility == Domain.Enums.PostVisibility.Private && post.AuthorId != userId)
            {
                return Result<CommentDto>.Failure("You cannot comment on this post.");
            }
            var replyComment = new Comment(parentComment.PostId, userId, content, parentCommentId);
            await _commentRepository.AddAsync(replyComment, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            replyComment = await _commentRepository.GetByIdIncludeAsync(replyComment.Id, ct) ?? replyComment;
            var dto = MapToDto(replyComment);
            return Result<CommentDto>.Success(dto);
        }
        // comment đơn lẻ
        private static CommentDto MapToDto(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                PostId = comment.PostId,
                AuthorId = comment.AuthorId,
                AuthorUsername = comment.Author?.DisplayName ?? comment.Author?.UserName ?? string.Empty,
                AvatarUrl = comment.Author?.AvatarUrl,
                ParentCommentId = comment.ParentCommentId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        // replies tree
        private static CommentDto MapToTreeDto(Comment comment)
        {
            var dto = MapToDto(comment);

            dto.Replies = comment.Replies?
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .Select(MapToTreeDto)
                .ToList()
                ?? new List<CommentDto>();

            return dto;
        }
    }
}

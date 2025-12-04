using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Comments.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public string? AuthorUsername { get; set; } = null;
        public string? AvatarUrl { get; set; } = null;
        public Guid? ParentCommentId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<CommentDto> Replies { get; set; } = new List<CommentDto>();

    }
}

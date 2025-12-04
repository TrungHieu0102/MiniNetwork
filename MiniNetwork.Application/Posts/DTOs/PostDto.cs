using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Posts.DTOs
{
    public class PostDto
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public string Content { get; set; } = null!;
        public string AuthorName { get; set; } = string.Empty;
        public IReadOnlyList<string> Images { get; set; } = Array.Empty<string>();
        public int CommentsCount { get; set; }
        public int LikesCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PostLikerDto> Likers { get; set; } = new();

    }
}

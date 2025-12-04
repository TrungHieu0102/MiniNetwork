

namespace MiniNetwork.Application.Posts.DTOs
{
    public class PostLikerDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }
}

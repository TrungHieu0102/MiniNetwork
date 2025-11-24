using MiniNetwork.Domain.Common;

namespace MiniNetwork.Domain.Entities;

public class PostLike : AuditableEntity
{
    public Guid PostId { get; private set; }
    public Guid UserId { get; private set; }

    public Post Post { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private PostLike() { }

    public PostLike(Guid postId, Guid userId)
    {
        PostId = postId;
        UserId = userId;
        MarkCreated(userId);
    }
}

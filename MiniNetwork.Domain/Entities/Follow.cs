using MiniNetwork.Domain.Common;

namespace MiniNetwork.Domain.Entities;

public class Follow : AuditableEntity
{
    public Guid FollowerId { get; private set; }
    public Guid FolloweeId { get; private set; }

    public User Follower { get; private set; } = null!;
    public User Followee { get; private set; } = null!;

    private Follow() { }

    public Follow(Guid followerId, Guid followeeId)
    {
        if (followerId == followeeId)
            throw new ArgumentException("User cannot follow themselves.", nameof(followeeId));

        FollowerId = followerId;
        FolloweeId = followeeId;
        MarkCreated(followerId);
    }
}

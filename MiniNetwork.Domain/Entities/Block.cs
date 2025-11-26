using MiniNetwork.Domain.Common;

namespace MiniNetwork.Domain.Entities;

public class Block : AuditableEntity
{
    public Guid BlockerId { get; private set; } 
    public Guid BlockedId { get; private set; } 

    public User Blocker { get; private set; } = null!;
    public User Blocked { get; private set; } = null!;

    private Block() { } 

    public Block(Guid blockerId, Guid blockedId)
    {
        if (blockerId == blockedId)
            throw new ArgumentException("User cannot block themselves.", nameof(blockedId));

        BlockerId = blockerId;
        BlockedId = blockedId;
        MarkCreated(blockerId);
    }
}

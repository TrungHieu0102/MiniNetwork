namespace MiniNetwork.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public Guid? CreatedById { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }
    public Guid? UpdatedById { get; protected set; }

    public void MarkCreated(Guid? userId)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedById = userId;
    }

    public void MarkUpdated(Guid? userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = userId;
    }
}

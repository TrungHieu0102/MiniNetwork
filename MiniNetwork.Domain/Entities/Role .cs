using MiniNetwork.Domain.Common;

namespace MiniNetwork.Domain.Entities;

public class Role : SoftDeletableEntity
{
    public string Name { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;
    public string? Description { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private Role() { }

    public Role(string name, string? description = null)
    {
        SetName(name);
        Description = description;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));

        Name = name.Trim();
        NormalizedName = Name.ToUpperInvariant();
    }

    public void SetDescription(string? description)
    {
        Description = description;
        MarkUpdated(null);
    }
}

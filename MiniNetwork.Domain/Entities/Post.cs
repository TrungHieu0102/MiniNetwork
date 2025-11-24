using MiniNetwork.Domain.Common;
using MiniNetwork.Domain.Enums;
using System.Xml.Linq;

namespace MiniNetwork.Domain.Entities;

public class Post : SoftDeletableEntity
{
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public PostVisibility Visibility { get; private set; } = PostVisibility.Public;

    public User Author { get; private set; } = null!;
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; private set; } = new List<PostLike>();

    private Post() { }

    public Post(Guid authorId, string content, string? imageUrl = null, PostVisibility visibility = PostVisibility.Public)
    {
        AuthorId = authorId;
        SetContent(content);
        ImageUrl = imageUrl;
        Visibility = visibility;
    }

    public void SetContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required.", nameof(content));

        Content = content;
        MarkUpdated(AuthorId);
    }

    public void SetVisibility(PostVisibility visibility)
    {
        Visibility = visibility;
        MarkUpdated(AuthorId);
    }

    public void SetImage(string? imageUrl)
    {
        ImageUrl = imageUrl;
        MarkUpdated(AuthorId);
    }
}

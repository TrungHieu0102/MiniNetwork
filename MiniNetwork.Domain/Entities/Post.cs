using MiniNetwork.Domain.Common;
using MiniNetwork.Domain.Enums;

namespace MiniNetwork.Domain.Entities;

public class Post : SoftDeletableEntity
{
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; } = null!;
    public PostVisibility Visibility { get; private set; } = PostVisibility.Public;

    public User Author { get; private set; } = null!;
    public ICollection<PostImage> Images { get; private set; } = new List<PostImage>();
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; private set; } = new List<PostLike>();

    private Post() { }

    public Post(Guid authorId, string content, PostVisibility visibility = PostVisibility.Public)
    {
        AuthorId = authorId;
        SetContent(content);
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

    public void AddImage(string url)
    {
        Images.Add(new PostImage(url));
        MarkUpdated(AuthorId);
    }

    public void RemoveImage(Guid imageId)
    {
        var img = Images.FirstOrDefault(i => i.Id == imageId);
        if (img is null) return;

        Images.Remove(img);
        MarkUpdated(AuthorId);
    }

    public void ClearImages()
    {
        Images.Clear();
        MarkUpdated(AuthorId);
    }
}

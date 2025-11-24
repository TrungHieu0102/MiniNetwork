using MiniNetwork.Domain.Common;

namespace MiniNetwork.Domain.Entities;

public class Comment : SoftDeletableEntity
{
    public Guid PostId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; } = null!;

    public Guid? ParentCommentId { get; private set; }

    public Post Post { get; private set; } = null!;
    public User Author { get; private set; } = null!;
    public Comment? ParentComment { get; private set; }
    public ICollection<Comment> Replies { get; private set; } = new List<Comment>();

    private Comment() { }

    public Comment(Guid postId, Guid authorId, string content, Guid? parentCommentId = null)
    {
        PostId = postId;
        AuthorId = authorId;
        SetContent(content);
        ParentCommentId = parentCommentId;
    }

    public void SetContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required.", nameof(content));

        Content = content;
        MarkUpdated(AuthorId);
    }
}

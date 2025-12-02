using MiniNetwork.Domain.Common;
using MiniNetwork.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Domain.Entities
{
    public class Notification : AuditableEntity
    {
        public Guid RecipientId { get; private set; } // user who recived the notiffication
        public Guid ActorId { get; private set; } //user who caused the notification
        public NotificationType Type { get; private set; }

        public Guid? PostId { get; private set; }
        public Guid? CommentId { get; private set; }

        public string Message { get; private set; } = null!;

        public bool IsRead { get; private set; }
        public DateTime? ReadAt { get; private set; }
        // Navigation properties
        public User Recipient { get; private set; } = null!;
        public User Actor { get; private set; } = null!;
        public Post? Post { get; private set; }
        public Comment? Comment { get; private set; }
        private Notification() { }
        public Notification(Guid recipientId, Guid actorId, NotificationType type, string message, Guid? postId, Guid? commentId)
        {
            if (recipientId == actorId)
                throw new ArgumentException("Recipient and Actor cannot be the same user.");
            RecipientId = recipientId;
            ActorId = actorId;
            Type = type;
            Message = message;
            PostId = postId;
            CommentId = commentId;

            IsRead = false;
            ReadAt = null;

            MarkCreated(actorId);
        }
        public void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
                ReadAt = DateTime.UtcNow;
                MarkUpdated(RecipientId);
            }

        }

    }
}

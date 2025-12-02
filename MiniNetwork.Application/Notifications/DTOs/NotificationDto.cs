using System;
using System.Collections.Generic;
using System.Text;

namespace MiniNetwork.Application.Notifications.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }

        public Guid ActorId { get; set; }
        public string ActorDisplayName { get; set; } = null!;
        public string? ActorAvatarUrl { get; set; }

        public Guid? PostId { get; set; }
        public Guid? CommentId { get; set; }
    }
}

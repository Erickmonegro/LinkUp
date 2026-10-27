using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Domain.Entities
{
    public class Notification : BasicEntity<int>
    {
        public required string ReceiverId { get; set; }
        public required string TriggerUserId { get; set; }
        public NotificationType ActivityType { get; set; }
        public required string Description { get; set; }
        public int? RelatedEntityId { get; set; }
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    }
}

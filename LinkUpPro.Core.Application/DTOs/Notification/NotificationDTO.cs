using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Application.DTOs.NotificationDTO
{
    public class NotificationDTO : BasicDTO<int>
    {
        public required string ReceiverId { get; set; }
        
        public required string TriggerUserId { get; set; }
        public required string TriggerUserName { get; set; }
        public string? TriggerUserPhotoUrl { get; set; }

        public NotificationType ActivityType { get; set; }
        public required string Description { get; set; }
        public int? RelatedEntityId { get; set; }
        public NotificationStatus Status { get; set; }
    }
}

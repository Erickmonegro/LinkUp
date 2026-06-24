using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Domain.Entities
{
    public class FriendRequest : BasicEntity<int>
    {
        public required string SenderId { get; set; }
        public required string ReceiverId { get; set; }
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
        public DateTime? RespondedAt { get; set; }
        public bool IsHiddenForSender { get; set; }
    }
}

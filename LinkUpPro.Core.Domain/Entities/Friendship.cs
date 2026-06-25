using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Domain.Entities
{
    public class Friendship : BasicEntity<int>
    {
        public required string UserId1 { get; set; }
        public required string UserId2 { get; set; }
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Active;
        public DateTime? DeletedAt { get; set; }
    }
}

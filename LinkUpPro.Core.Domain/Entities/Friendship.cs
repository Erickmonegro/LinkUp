using LinkUpPro.Core.Domain.Common;

namespace LinkUpPro.Core.Domain.Entities
{
    public class Friendship : BasicEntity<int>
    {
        public required string UserId1 { get; set; }
        public required string UserId2 { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

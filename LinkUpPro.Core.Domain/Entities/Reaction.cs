using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Domain.Entities
{
    public class Reaction : BasicEntity<int>
    {
        public ReactionType ReactionType { get; set; }
        public required string UserId { get; set; }
        public int PostId { get; set; }
        // Navigation 
        public Post? Post { get; set; }
    }
}

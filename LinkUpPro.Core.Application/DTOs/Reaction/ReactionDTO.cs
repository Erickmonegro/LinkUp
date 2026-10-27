using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Application.DTOs.ReactionDTO
{
    public class ReactionDTO : BasicDTO<int>
    {
        public int PostId { get; set; }
        public ReactionType ReactionType { get; set; }

        public required string UserId { get; set; }
        public required string UserName { get; set; }
    }
}

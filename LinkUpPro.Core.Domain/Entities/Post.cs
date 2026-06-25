using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Domain.Entities
{
    public class Post : BasicEntity<int>
    {
        public required string TextContent { get; set; }
        public ContentType ContentType { get; set; }
        public string? MediaUrl { get; set; }
        public PrivacyMode PrivacyMode { get; set; } = PrivacyMode.FriendsOnly;
        public CommentStatus CommentStatus { get; set; } = CommentStatus.Allowed;
        public EditionStatus EditionStatus { get; set; } = EditionStatus.NotEdited;
        public required string AuthorId { get; set; }

        // Navigation 
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}

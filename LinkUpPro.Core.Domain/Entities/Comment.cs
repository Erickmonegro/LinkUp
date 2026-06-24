using LinkUpPro.Core.Domain.Common;

namespace LinkUpPro.Core.Domain.Entities
{
    public class Comment : BasicEntity<int>
    {
        public required string Content { get; set; }
        public required string AuthorId { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        // Navigation 
        public Post? Post { get; set; }
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}

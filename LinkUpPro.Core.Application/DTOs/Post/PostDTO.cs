using LinkUpPro.Core.Application.DTOs.CommentDTO;
using LinkUpPro.Core.Domain.Common.Enums;
using System.Collections.Generic;

namespace LinkUpPro.Core.Application.DTOs.PostDTO
{
    public class PostDTO : BasicDTO<int>
    {
        public required string TextContent { get; set; }
        public ContentType ContentType { get; set; }
        public string? MediaUrl { get; set; }
        public PrivacyMode PrivacyMode { get; set; }
        public CommentStatus CommentStatus { get; set; }
        public EditionStatus EditionStatus { get; set; }

        public required string AuthorId { get; set; }
        public required string AuthorName { get; set; }
        public string? AuthorPhotoUrl { get; set; }

        public int CommentsCount { get; set; }
        public int ReactionsCount { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }

        public ICollection<CommentDTO.CommentDTO> Comments { get; set; } = new List<CommentDTO.CommentDTO>();
        public ICollection<ReactionDTO.ReactionDTO> Reactions { get; set; } = new List<ReactionDTO.ReactionDTO>();
    }
}

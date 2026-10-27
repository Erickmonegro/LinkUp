using System;
using System.Collections.Generic;

namespace LinkUpPro.Core.Application.DTOs.CommentDTO
{
    public class CommentDTO : BasicDTO<int>
    {
        public required string Content { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }

        public required string AuthorId { get; set; }
        public required string AuthorName { get; set; }
        public string? AuthorUserName { get; set; }
        public string? AuthorPhotoUrl { get; set; }
        public bool IsEdited { get; set; }

        public ICollection<CommentDTO> Replies { get; set; } = new List<CommentDTO>();
    }
}

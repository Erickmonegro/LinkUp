using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Comment
{
    public class CommentViewModel : BasicViewModel<int>
    {
        public int PostId { get; set; }
        
        [Required]
        public string AuthorId { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Autor")]
        public string AuthorName { get; set; } = string.Empty;
        
        public string? AuthorPhotoUrl { get; set; }
        
        [Required]
        [Display(Name = "Comentario")]
        public string Content { get; set; } = string.Empty;
        
        [Display(Name = "Fecha")]
        public DateTime CreatedAt { get; set; }
        
        public int? ParentCommentId { get; set; }
        
        public List<CommentViewModel> Replies { get; set; } = new List<CommentViewModel>();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LinkUpPro.Core.Domain.Common.Enums;
using LinkUpPro.Core.Application.ViewModels.Comment;
using LinkUpPro.Core.Application.ViewModels.Reaction;

namespace LinkUpPro.Core.Application.ViewModels.Post
{
    public class PostViewModel : BasicViewModel<int>
    {
        [Required]
        public string AuthorId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Autor")]
        public string AuthorName { get; set; } = string.Empty;

        public string? AuthorPhotoUrl { get; set; }

        [Required]
        [Display(Name = "Contenido")]
        public string TextContent { get; set; } = string.Empty;

        public string? MediaUrl { get; set; }

        [Required]
        [Display(Name = "Tipo de Contenido")]
        public ContentType ContentType { get; set; }

        [Display(Name = "Privacidad")]
        public PrivacyMode PrivacyMode { get; set; }

        [Display(Name = "Estado de Comentarios")]
        public CommentStatus CommentStatus { get; set; }

        [Display(Name = "Estado de Edición")]
        public EditionStatus EditionStatus { get; set; }

        [Display(Name = "Fecha de Publicación")]
        public DateTime CreatedAt { get; set; }

        public int CommentsCount { get; set; }
        public int ReactionsCount { get; set; }

        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
        public List<ReactionViewModel> Reactions { get; set; } = new List<ReactionViewModel>();
    }
}

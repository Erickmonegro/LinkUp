using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Friendship
{
    public class ReactionSummary
    {
        public string ReactionType { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class FriendPostViewModel : BasicViewModel<int>
    {
        [Required(ErrorMessage = "El contenido es requerido.")]
        [Display(Name = "Contenido")]
        public string TextContent { get; set; } = string.Empty;

        public string? MediaUrl { get; set; }

        [Required(ErrorMessage = "El tipo de contenido es requerido.")]
        public string ContentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "El autor es requerido.")]
        [Display(Name = "Autor")]
        public string AuthorName { get; set; } = string.Empty;

        public string? AuthorProfilePhoto { get; set; }

        [Required(ErrorMessage = "El Id del autor es requerido.")]
        public string AuthorId { get; set; } = string.Empty;

        [Display(Name = "Fecha de Publicación")]
        public DateTime CreatedAt { get; set; }

        public bool IsEdited { get; set; }

        [Display(Name = "Comentarios")]
        public int CommentsCount { get; set; }

        [Display(Name = "Reacciones")]
        public int ReactionsCount { get; set; }

        public List<ReactionSummary> Reactions { get; set; } = new List<ReactionSummary>();
    }
}

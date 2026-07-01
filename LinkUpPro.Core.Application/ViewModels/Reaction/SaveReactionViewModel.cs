using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Reaction
{
    public class SaveReactionViewModel
    {
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        
        [Required(ErrorMessage = "El tipo de reacción es requerido.")]
        public string ReactionType { get; set; } = string.Empty;
    }
}

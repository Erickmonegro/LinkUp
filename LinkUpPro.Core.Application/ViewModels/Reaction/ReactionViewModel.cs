using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Reaction
{
    public class ReactionViewModel : BasicViewModel<int>
    {
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        
        [Required]
        public string AuthorId { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Autor")]
        public string AuthorName { get; set; } = string.Empty;
        
        [Required]
        public string ReactionType { get; set; } = string.Empty;
    }
}

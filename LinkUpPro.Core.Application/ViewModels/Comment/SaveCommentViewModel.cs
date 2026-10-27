using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Comment
{
    public class SaveCommentViewModel : BasicViewModel<int>
    {
        [Required(ErrorMessage = "El PostId es requerido.")]
        public int PostId { get; set; }
        
        [Required(ErrorMessage = "El comentario no puede estar vacío.")]
        [MaxLength(500, ErrorMessage = "El comentario debe tener un máximo de 500 caracteres.")]
        public string Content { get; set; } = string.Empty;
        
        public int? ParentCommentId { get; set; }
    }
}

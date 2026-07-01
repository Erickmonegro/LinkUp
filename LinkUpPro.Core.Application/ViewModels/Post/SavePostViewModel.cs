using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Application.ViewModels.Post
{
    public class SavePostViewModel : BasicViewModel<int>
    {
        [Required(ErrorMessage = "Debe ingresar el contenido de la publicación.")]
        [Display(Name = "Contenido")]
        [MaxLength(1000, ErrorMessage = "El contenido debe tener un máximo de 1000 caracteres.")]
        public string TextContent { get; set; } = string.Empty;

        [Display(Name = "Enlace de YouTube")]
        public string? MediaUrl { get; set; }

        [Required(ErrorMessage = "El tipo de contenido es requerido.")]
        [Display(Name = "Tipo de Contenido")]
        public ContentType ContentType { get; set; }

        [Required(ErrorMessage = "La privacidad es requerida.")]
        [Display(Name = "Privacidad")]
        public PrivacyMode PrivacyMode { get; set; } = PrivacyMode.FriendsOnly;

        [Display(Name = "Permitir comentarios")]
        public CommentStatus CommentStatus { get; set; } = CommentStatus.Allowed;

        public EditionStatus EditionStatus { get; set; } = EditionStatus.NotEdited;

        public string? AuthorId { get; set; }

        [Display(Name = "Cargar Imagen")]
        public IFormFile? ImageFile { get; set; }
    }
}

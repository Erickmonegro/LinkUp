using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using LinkUpPro.Core.Domain.Common.Enums;
using LinkUpPro.Core.Application.Helpers.Validations;

namespace LinkUpPro.Core.Application.ViewModels.Post
{
    public class SavePostViewModel : BasicViewModel<int>, IValidatableObject
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
        [ValidImageFile]
        public IFormFile? ImageFile { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ContentType == ContentType.Image)
            {
                // Solo requiere imagen si es una publicación nueva (Id == 0)
                if (Id == 0 && (ImageFile == null || ImageFile.Length == 0))
                {
                    yield return new ValidationResult("Debe seleccionar una imagen para crear la publicación.", new[] { nameof(ImageFile) });
                }
            }
            else if (ContentType == ContentType.YouTubeVideo)
            {
                var youtubeId = GetYouTubeVideoId(MediaUrl ?? "");
                if (string.IsNullOrWhiteSpace(MediaUrl) || youtubeId == null)
                {
                    yield return new ValidationResult("Debe ingresar un enlace válido de YouTube.", new[] { nameof(MediaUrl) });
                }
                else
                {
                    MediaUrl = $"https://www.youtube.com/embed/{youtubeId}";
                }
            }
        }

        private static string? GetYouTubeVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;

            var host = uri.Host.ToLower();
            if (!host.Contains("youtube.com") && !host.Contains("youtu.be")) return null;

            if (host.Contains("youtu.be")) return uri.AbsolutePath.TrimStart('/');

            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("v", out var videoId)) return videoId.FirstOrDefault();

            var segments = uri.Segments;
            if (segments.Length > 1 && (segments[1] == "embed/" || segments[1] == "v/")) return segments[^1];

            return null;
        }
    }
}
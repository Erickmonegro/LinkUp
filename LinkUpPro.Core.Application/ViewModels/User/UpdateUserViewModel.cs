using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.User
{
    public class UpdateUserViewModel : BasicViewModel<string>
    {
        [Required(ErrorMessage = "Debe ingresar su nombre")]
        [Display(Name = "Nombre")]
        [DataType(DataType.Text)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Debe ingresar su apellido")]
        [Display(Name = "Apellido")]
        [DataType(DataType.Text)]
        public required string LastName { get; set; }

        // Note: Read-only in UI per PDF, but we keep validation
        [Required(ErrorMessage = "Debe ingresar su correo electrónico")]
        [Display(Name = "Correo Electrónico")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        // Note: Read-only in UI per PDF, but we keep validation
        [Required(ErrorMessage = "Debe ingresar un nombre de usuario")]
        [Display(Name = "Nombre de Usuario")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }

        [Display(Name = "Contraseña Actual (Obligatoria si desea cambiarla)")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Display(Name = "Nueva Contraseña (Opcional)")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "La nueva contraseña y su confirmación no coinciden")]
        [Display(Name = "Confirmar Nueva Contraseña")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Debe ingresar su teléfono")]
        [Display(Name = "Teléfono")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^(?:\+?1[-. ]?)?\(?(809|829|849)\)?[-. ]?\d{3}[-. ]?\d{4}$", ErrorMessage = "Formato de teléfono dominicano inválido (ej. 809-555-1234 o 8095551234).")]
        public string? PhoneNumber { get; set; }

        public string? PhotoUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }

        [Display(Name = "Actualizar Foto de Perfil (Opcional)")]
        [DataType(DataType.Upload)]
        public IFormFile? ProfileImageFile { get; set; }

        [Display(Name = "Actualizar Foto de Portada (Opcional)")]
        [DataType(DataType.Upload)]
        public IFormFile? CoverImageFile { get; set; }
    }
}

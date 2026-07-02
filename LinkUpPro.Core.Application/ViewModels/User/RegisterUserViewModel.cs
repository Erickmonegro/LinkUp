using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using LinkUpPro.Core.Application.Helpers.Validations;

namespace LinkUpPro.Core.Application.ViewModels.User
{
    public class RegisterUserViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su nombre")]
        [Display(Name = "Nombre")]
        [DataType(DataType.Text)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Debe ingresar su apellido")]
        [Display(Name = "Apellido")]
        [DataType(DataType.Text)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Debe ingresar su correo electrónico")]
        [Display(Name = "Correo Electrónico")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Debe ingresar un nombre de usuario")]
        [Display(Name = "Nombre de Usuario")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Debe ingresar su contraseña")]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "La contraseña y su confirmación no coinciden")]
        [Required(ErrorMessage = "Debe confirmar su contraseña")]
        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        public required string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Debe ingresar su teléfono")]
        [Display(Name = "Teléfono")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^(?:\+?1[-. ]?)?\(?(809|829|849)\)?[-. ]?\d{3}[-. ]?\d{4}$", ErrorMessage = "Formato de teléfono dominicano inválido (ej. 809-555-1234 o 8095551234).")]
        public required string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una foto de perfil")]
        [Display(Name = "Foto de Perfil")]
        [DataType(DataType.Upload)]
        [ValidImageFile]
        public IFormFile? ProfileImageFile { get; set; }
    }
}

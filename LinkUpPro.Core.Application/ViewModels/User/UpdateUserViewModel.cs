using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LinkUpPro.Core.Application.Helpers.Validations;

namespace LinkUpPro.Core.Application.ViewModels.User
{
    public class UpdateUserViewModel : BasicViewModel<string>, IValidatableObject
    {
        [Required(ErrorMessage = "Debe ingresar su nombre.")]
        [Display(Name = "Nombre")]
        [DataType(DataType.Text)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Debe ingresar su apellido.")]
        [Display(Name = "Apellido")]
        [DataType(DataType.Text)]
        public required string LastName { get; set; }

        [Display(Name = "Correo Electrónico")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Display(Name = "Nombre de Usuario")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }

        [Display(Name = "Contraseña actual")]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Display(Name = "Nueva contraseña")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "La nueva contraseña y su confirmación no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Debe ingresar un número telefónico válido de República Dominicana.")]
        [Display(Name = "Teléfono")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^(?:\+?1[-. ]?)?\(?(809|829|849)\)?[-. ]?\d{3}[-. ]?\d{4}$", ErrorMessage = "Debe ingresar un número telefónico válido de República Dominicana.")]
        public string? PhoneNumber { get; set; }

        public string? PhotoUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }

        [Display(Name = "Foto de perfil")]
        [DataType(DataType.Upload)]
        [ValidImageFile]
        public IFormFile? ProfileImageFile { get; set; }

        [Display(Name = "Foto de Portada")]
        [DataType(DataType.Upload)]
        [ValidImageFile]
        public IFormFile? CoverImageFile { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            bool hasCurrent = !string.IsNullOrWhiteSpace(CurrentPassword);
            bool hasNew = !string.IsNullOrWhiteSpace(Password);
            bool hasConfirm = !string.IsNullOrWhiteSpace(ConfirmPassword);

            // Si completa cualquiera de los campos de contraseña, debe completar los tres
            if (hasCurrent || hasNew || hasConfirm)
            {
                if (!hasCurrent || !hasNew || !hasConfirm)
                {
                    yield return new ValidationResult(
                        "Para cambiar su contraseña debe completar la contraseña actual, la nueva contraseña y su confirmación.",
                        new[] { nameof(CurrentPassword) });
                }
                else
                {
                    if (CurrentPassword == Password)
                    {
                        yield return new ValidationResult(
                            "La nueva contraseña debe ser diferente de la contraseña actual.",
                            new[] { nameof(Password) });
                    }
                }
            }
        }
    }
}

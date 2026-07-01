using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.User
{
    public class ResetPasswordRequestViewModel
    {
        [Required]
        public required string UserId { get; set; }

        [Required]
        public required string Token { get; set; }

        [Required(ErrorMessage = "Debe ingresar su nueva contraseña")]
        [Display(Name = "Nueva Contraseña")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "La contraseña y su confirmación no coinciden")]
        [Required(ErrorMessage = "Debe confirmar su contraseña")]
        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        public required string ConfirmPassword { get; set; }
    }
}

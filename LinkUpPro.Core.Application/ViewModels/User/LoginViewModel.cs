using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.User
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        [Display(Name = "Nombre de usuario")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Display(Name = "Mantener sesión iniciada")]
        public bool RememberMe { get; set; }
    }
}

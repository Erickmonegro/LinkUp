using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.User
{
    public class ForgotPasswordRequestViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su usuario o correo electrónico")]
        [Display(Name = "Usuario o Correo Electrónico")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }
    }
}

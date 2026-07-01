using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.User
{
    public class ForgotPasswordRequestViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su nombre de usuario")]
        [Display(Name = "Nombre de Usuario")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }
    }
}

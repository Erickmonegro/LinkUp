using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.ViewModels.User
{
    public class CreateUserViewModel
    {
        public required int Id { get; set; }

        [Required(ErrorMessage = "Debe ingresar el nombre")]
        public required string UserName { get; set; } 
    }
}

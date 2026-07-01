using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.ViewModels.Battleship
{
    public class CreateMatchViewModel
    {
        [Required(ErrorMessage = "Debes seleccionar un amigo para jugar")]
        [Display(Name = "Jugador a Retar")]
        public string Player2Id { get; set; } = string.Empty;
    }
}
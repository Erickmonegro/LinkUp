using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.ViewModels.Battleship
{
    public class SurrenderViewModel
    {
        [Required(ErrorMessage = "La partida es requerida")]
        [Display(Name = "Partida ID")]
        public int MatchId { get; set; }

        [Required(ErrorMessage = "El jugador es requerido")]
        [Display(Name = "Jugador ID")]
        public string PlayerId { get; set; } = string.Empty;
    }
}
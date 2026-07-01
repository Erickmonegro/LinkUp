using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.ViewModels.Battleship
{
    public class AttackViewModel : BasicViewModel<int>
    {
        [Required(ErrorMessage = "El ID de la partida es requerido")]
        [Display(Name = "Partida ID")]
        public int MatchId { get; set; }

        [Required(ErrorMessage = "El atacante es requerido")]
        [Display(Name = "Atacante")]
        public required string AttackerId { get; set; }

        [Required(ErrorMessage = "La coordenada X es requerida")]
        [Display(Name = "Coordenada X")]
        public int X { get; set; }

        [Required(ErrorMessage = "La coordenada Y es requerida")]
        [Display(Name = "Coordenada Y")]
        public int Y { get; set; }

        [Display(Name = "Es un acierto")]
        public bool IsHit { get; set; }
    }
}
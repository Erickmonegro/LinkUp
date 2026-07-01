using LinkUpPro.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.ViewModels.Battleship
{
    public class ShipPositionViewModel : BasicViewModel<int>
    {
        [Required(ErrorMessage = "El ID de la partida es requerido")]
        [Display(Name = "Partida ID")]
        public int BattleshipMatchId { get; set; }

        [Required(ErrorMessage = "El usuario es requerido")]
        [Display(Name = "Usuario")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "El tamaño del barco es requerido")]
        [Display(Name = "Tamaño del barco")]
        public int ShipSize { get; set; }

        [Required(ErrorMessage = "La coordenada X de inicio es requerida")]
        [Display(Name = "Posición X Inicial")]
        public int StartX { get; set; }

        [Required(ErrorMessage = "La coordenada Y de inicio es requerida")]
        [Display(Name = "Posición Y Inicial")]
        public int StartY { get; set; }

        [Required(ErrorMessage = "La dirección del barco es requerida")]
        [Display(Name = "Dirección")]
        public ShipDirection Direction { get; set; }

        [Display(Name = "Hundido")]
        public bool IsSunk { get; set; }

        [Display(Name = "Aciertos")]
        public int Hits { get; set; }
    }
}
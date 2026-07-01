using LinkUpPro.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.ViewModels.Battleship
{
    public class BattleshipMatchViewModel : BasicViewModel<int>
    {
        [Required(ErrorMessage = "El Jugador 1 es requerido")]
        [Display(Name = "Jugador 1")]
        public required string Player1Id { get; set; }

        [Required(ErrorMessage = "El Jugador 2 es requerido")]
        [Display(Name = "Jugador 2")]
        public required string Player2Id { get; set; }

        [Display(Name = "Estado de la partida")]
        public MatchStatus Status { get; set; }

        [Display(Name = "Ganador")]
        public string? WinnerId { get; set; }

        [Display(Name = "Fecha de finalización")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "El turno actual es requerido")]
        [Display(Name = "Turno actual")]
        public required string CurrentTurnUserId { get; set; }

        [Display(Name = "Fecha de asignación de turno")]
        public DateTime TurnAssignedAt { get; set; }
    }
}
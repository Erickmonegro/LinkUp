using LinkUpPro.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.DTOs.Battleship
{
    public class BattleshipMatchDTO : BasicDTO<int>
    {
        public required string Player1Id { get; set; }
        public required string Player2Id { get; set; }
        public MatchStatus Status { get; set; } = MatchStatus.InProgress;
        public string? WinnerId { get; set; }
        public DateTime? EndDate { get; set; }
        public required string CurrentTurnUserId { get; set; }
        public DateTime TurnAssignedAt { get; set; }
    }
}
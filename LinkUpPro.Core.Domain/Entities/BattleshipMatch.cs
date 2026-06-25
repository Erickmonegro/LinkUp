using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Domain.Entities
{
    public class BattleshipMatch : BasicEntity<int>
    {
        public required string Player1Id { get; set; }
        public required string Player2Id { get; set; }
        public MatchStatus Status { get; set; } = MatchStatus.InProgress;
        public string? WinnerId { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime TurnAssignedAt { get; set; }
        public required string CurrentTurnUserId { get; set; }

        // Navigation 
        public ICollection<ShipPosition> Ships { get; set; } = new List<ShipPosition>();
        public ICollection<Attack> Attacks { get; set; } = new List<Attack>();
    }
}

using LinkUpPro.Core.Domain.Common;

namespace LinkUpPro.Core.Domain.Entities
{
    public class Attack : BasicEntity<int>
    {
        public int MatchId { get; set; }
        public required string AttackerId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsHit { get; set; }

        // Navigation 
        public BattleshipMatch? Match { get; set; }
    }
}

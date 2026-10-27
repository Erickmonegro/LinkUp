using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Domain.Entities
{
    public class ShipPosition : BasicEntity<int>
    {
        public int BattleshipMatchId { get; set; }
        public required string UserId { get; set; }
        public int ShipSize { get; set; } // e.g. 2, 3, 4, 5
        public int StartX { get; set; }
        public int StartY { get; set; }
        public ShipDirection Direction { get; set; }
        public bool IsSunk { get; set; }
        public int Hits { get; set; }

        // Navigation 
        public BattleshipMatch? BattleshipMatch { get; set; }
    }
}

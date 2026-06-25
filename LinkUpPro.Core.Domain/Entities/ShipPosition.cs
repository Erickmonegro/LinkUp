using LinkUpPro.Core.Domain.Common;
using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Domain.Entities
{
    public class ShipPosition : BasicEntity<int>
    {
        public int MatchId { get; set; }
        public required string UserId { get; set; }
        public int ShipSize { get; set; } // e.g. 2, 3, 4, 5
        public int StartX { get; set; }
        public int StartY { get; set; }
        public ShipDirection Direction { get; set; }
        public bool IsSunk { get; set; }

        // Navigation 
        public Battleship? BattleshipMatch { get; set; }
    }
}

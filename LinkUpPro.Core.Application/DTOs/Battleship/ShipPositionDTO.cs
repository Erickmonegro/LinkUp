using LinkUpPro.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.DTOs.Battleship
{
    public class ShipPositionDTO : BasicDTO<int>
    {
        public int BattleshipMatchId { get; set; }
        public required string UserId { get; set; }
        public int ShipSize { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public ShipDirection Direction { get; set; }
        public bool IsSunk { get; set; }
        public int Hits { get; set; }
    }
}
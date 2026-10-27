using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.DTOs.Battleship
{
    public class AttackDTO : BasicDTO<int>
    {
        public int MatchId { get; set; }
        public required string AttackerId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsHit { get; set; }
    }
}
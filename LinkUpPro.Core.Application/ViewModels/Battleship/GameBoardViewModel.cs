using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.ViewModels.Battleship
{
    public class GameBoardViewModel
    {
        public BattleshipMatchViewModel? Match { get; set; } = default;

        public List<ShipPositionViewModel> Ships { get; set; } = new();

        public List<AttackViewModel> Attacks { get; set; } = new();

        public bool IsMyTurn { get; set; }
    }
}
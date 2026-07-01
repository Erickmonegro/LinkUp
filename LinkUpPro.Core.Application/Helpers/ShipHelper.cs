using LinkUpPro.Core.Domain.Common.Enums;
using LinkUpPro.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Helpers
{
    public class ShipHelper
    {
        public static List<(int X, int Y)> GetShipCoordinates(ShipPosition ship)
        {
            List<(int X, int Y)> coordinates = new();

            for (int i = 0; i < ship.ShipSize; i++)
            {
                switch (ship.Direction)
                {
                    case ShipDirection.Up:
                        coordinates.Add((ship.StartX, ship.StartY - i));
                        break;

                    case ShipDirection.Down:
                        coordinates.Add((ship.StartX, ship.StartY + i));
                        break;

                    case ShipDirection.Left:
                        coordinates.Add((ship.StartX - i, ship.StartY));
                        break;

                    case ShipDirection.Right:
                        coordinates.Add((ship.StartX + i, ship.StartY));
                        break;
                }
            }

            return coordinates;
        }

        public static bool IsHit(ShipPosition ship, int x, int y)
        {
            return GetShipCoordinates(ship)
                .Any(c => c.X == x && c.Y == y);
        }

        public static bool Overlap(ShipPosition ship1, ShipPosition ship2)
        {
            var first = GetShipCoordinates(ship1);
            var second = GetShipCoordinates(ship2);

            return first.Any(c => second.Contains(c));
        }

        public static bool IsInsideBoard(ShipPosition ship, int boardSize)
        {
            return GetShipCoordinates(ship)
                .All(c => c.X >= 0 &&
                          c.X < boardSize &&
                          c.Y >= 0 &&
                          c.Y < boardSize);
        }
    }
}
using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.Helpers;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Common.Enums;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Services
{
    public class ShipPositionService : IShipPositionService
    {
        private readonly IShipPositionRepository repo;
        private readonly IMapper mapper;
        public ShipPositionService(IShipPositionRepository repo, IMapper mapper)
        {
            this.repo = repo;
            this.mapper = mapper;
        }

        public async Task<bool> AreShipsPlacedAsync(int matchId, string playerId)
        {
            var ships = await repo.GetByPlayerAsync(matchId, playerId);

            return ships.Count >= 5;
        }

        public async Task<List<ShipPositionDTO>> GetByMatchAsync(int matchId)
        {
            var ships = await repo.GetByMatchIdAsync(matchId);

            return mapper.Map<List<ShipPositionDTO>>(ships);
        }

        public async Task<List<ShipPositionDTO>> GetByPlayerAsync(int matchId, string playerId)
        {
            var ships = await repo.GetByPlayerAsync(matchId, playerId);

            return mapper.Map<List<ShipPositionDTO>>(ships);
        }

        public async Task<ShipPositionDTO> PlaceShipsAsync(int matchId, string playerId, ShipPositionDTO shipDto)
        {
            var ships = await repo.GetByPlayerAsync(matchId, playerId);

            // Ya colocó los 5 barcos
            if (ships.Count >= 5)
                throw new Exception("Ya colocaste todos los barcos.");

            var ship = mapper.Map<ShipPosition>(shipDto);

            ship.BattleshipMatchId = matchId;
            ship.UserId = playerId;
            ship.Hits = 0;
            ship.IsSunk = false;

            // Validar tamaño permitido
            ValidateShipSize(ship, ships);

            // Validar tablero
            if (!ShipHelper.IsInsideBoard(ship, 12))
                throw new Exception("El barco queda fuera del tablero.");

            // Validar superposicion
            foreach (var existing in ships)
            {
                if (ShipHelper.Overlap(ship, existing))
                    throw new Exception("Debe cambiar la celda seleccionada o la dirección, ya que con la combinación actual el barco quedaría posicionado encima de otro barco.");
            }

            ship = await repo.AddAsync(ship);

            return mapper.Map<ShipPositionDTO>(ship);
        }

        private void ValidateShipSize(ShipPosition ship, List<ShipPosition> ships)
        {
            Dictionary<int, int> fleet = new()
            {
                {2,1},
                {3,2},
                {4,1},
                {5,1}
            };

            if (!fleet.ContainsKey(ship.ShipSize))
                throw new Exception("Tamaño de barco inválido.");

            int current = ships.Count(x => x.ShipSize == ship.ShipSize);

            if (current >= fleet[ship.ShipSize])
                throw new Exception($"Ya colocaste todos los barcos de tamaño {ship.ShipSize}.");
        }
    }
}

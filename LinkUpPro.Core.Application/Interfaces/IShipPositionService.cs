using LinkUpPro.Core.Application.DTOs.Battleship;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IShipPositionService
    {
        Task<ShipPositionDTO> PlaceShipsAsync(int matchId, string playerId, ShipPositionDTO ships);
        Task<List<ShipPositionDTO>> GetByMatchAsync(int matchId);
        Task<List<ShipPositionDTO>> GetByPlayerAsync(int matchId, string playerId);
        Task<bool> AreShipsPlacedAsync(int matchId, string playerId);
    }
}
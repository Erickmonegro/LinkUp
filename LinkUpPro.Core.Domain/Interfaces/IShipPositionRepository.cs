using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IShipPositionRepository : IGenericRepository<ShipPosition>
    {
        Task<List<ShipPosition>> GetByMatchIdAsync(int battleshipMatchId);
        Task<List<ShipPosition>> GetByPlayerAsync(int battleshipMatchId, string userId);
    }
}

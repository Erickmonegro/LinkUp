using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IAttackRepository : IGenericRepository<Attack>
    {
        Task<List<Attack>> GetByMatchAsync(int battleshipMatchId);
        Task<bool> ExistsAttackAsync(int battleshipMatchId, string attackerId, int startX, int startY);
    }
}

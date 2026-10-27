using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IBattleshipMatchRepository : IGenericRepository<BattleshipMatch>
    {
        Task<BattleshipMatch?> GetActiveMatchBetweenUsersAsync(string userId1, string userId2);
        Task<List<BattleshipMatch>> GetActiveMatchesAsync(string userId);
        Task<List<BattleshipMatch>> GetMatchHistoryForUserAsync(string userId);
        Task<List<BattleshipMatch>> GetEndedMatchesAsync();
    }
}

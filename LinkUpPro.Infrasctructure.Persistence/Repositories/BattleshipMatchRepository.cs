using LinkUpPro.Core.Domain.Common.Enums;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;


namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class BattleshipMatchRepository : GenericRepository<BattleshipMatch>, IBattleshipMatchRepository

    {
        private readonly LinkUpAppContext repo;
        public BattleshipMatchRepository(LinkUpAppContext _context) : base(_context)
        {
            repo = _context;
        }

        public async Task<BattleshipMatch?> GetActiveMatchBetweenUsersAsync(string userId1, string userId2)
        {
            return await repo.Set<BattleshipMatch>()
                .FirstOrDefaultAsync(m => m.Status == MatchStatus.InProgress && 
                    ((m.Player1Id == userId1 && m.Player2Id == userId2) || 
                     (m.Player1Id == userId2 && m.Player2Id == userId1)));
        }

        public async Task<List<BattleshipMatch>> GetActiveMatchesAsync(string userId)
        {
            return await repo.Set<BattleshipMatch>()
                .Where(m => m.Status == MatchStatus.InProgress && (m.Player1Id == userId || m.Player2Id == userId))
                .ToListAsync();
        }

        public async Task<List<BattleshipMatch>> GetEndedMatchesAsync()
        {
            return await repo.Set<BattleshipMatch>()
                .Where(m => m.Status == MatchStatus.Finished)
                .ToListAsync();
        }

        public async Task<List<BattleshipMatch>> GetMatchHistoryForUserAsync(string userId)
        {
            return await repo.Set<BattleshipMatch>()
                .Where(m => m.Player1Id == userId || m.Player2Id == userId)
                .OrderBy(m => m.Status == MatchStatus.Finished ? 2 : (m.Status == MatchStatus.Pending ? 1 : 0))
                .ThenByDescending(m => m.Id)
                .ToListAsync();
        }
    }
}

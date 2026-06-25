using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class AttackRepository : GenericRepository<Attack>, IAttackRepository
    {
        private readonly LinkUpAppContext repo;
        public AttackRepository(LinkUpAppContext _context) : base(_context)
        {
            repo = _context;
        }

        public async Task<bool> ExistsAttackAsync(int battleshipMatchId, string attackerId, int startX, int startY)
        {
            return await repo.Set<Attack>()
                .AnyAsync(a => a.MatchId == battleshipMatchId && a.AttackerId == attackerId && a.X == startX && a.Y == startY);
        }

        public async Task<List<Attack>> GetByMatchAsync(int battleshipMatchId)
        {
            return await repo.Set<Attack>()
                .Where(a => a.MatchId == battleshipMatchId)
                .ToListAsync();
        }
    }
}

using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class ShipPositionRepository : GenericRepository<ShipPosition>, IShipPositionRepository
    {
        private readonly LinkUpAppContext repo;
        public ShipPositionRepository(LinkUpAppContext _context) : base(_context)
        {
            repo = _context;
        }

        public async Task<List<ShipPosition>> GetByMatchIdAsync(int battleshipMatchId)
        {
            return await repo.Set<ShipPosition>()
                .Where(a => a.BattleshipMatchId == battleshipMatchId)
                .ToListAsync();
        }

        public async Task<List<ShipPosition>> GetByPlayerAsync(int battleshipMatchId, string userId)
        {
            return await repo.Set<ShipPosition>()
               .Where(s => s.BattleshipMatchId == battleshipMatchId && s.UserId == userId)
               .ToListAsync();
        }
    }
}

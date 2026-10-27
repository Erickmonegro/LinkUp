using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
   public class ReactionRepository : GenericRepository<Reaction>, IReactionRepository
    {
        private readonly LinkUpAppContext repo;
        public ReactionRepository(LinkUpAppContext context) : base(context)
        {
            repo = context;
        }

        public async Task<Reaction?> GetReactionByUserAndPostAsync(string userId, int postId)
        {
            return await repo.Set<Reaction>()
                .FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);
        }
    }
}

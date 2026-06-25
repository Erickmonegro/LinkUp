using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using LinkUpPro.Core.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class FriendshipRepository : GenericRepository<Friendship>, IFriendshipRepository
    {
        private readonly LinkUpAppContext repo;
        public FriendshipRepository(LinkUpAppContext context) : base(context)
        {
            repo = context;
        }

        public async Task<bool> AreFriendsAsync(string userId1, string userId2)
        {
            return await repo.Set<Friendship>()
                .AnyAsync(f => f.Status == FriendshipStatus.Active && 
                    ((f.UserId1 == userId1 && f.UserId2 == userId2) || (f.UserId1 == userId2 && f.UserId2 == userId1)));
        }

        public async Task<List<string>> GetActiveFriendIdsAsync(string userId)
        {
            var friendships = await repo.Set<Friendship>()
                .Where(f => f.Status == FriendshipStatus.Active && (f.UserId1 == userId || f.UserId2 == userId))
                .ToListAsync();

            return friendships.Select(f => f.UserId1 == userId ? f.UserId2 : f.UserId1).Distinct().ToList();
        }

        public async Task<int> GetMutualFriendsCountAsync(string userId1, string userId2)
        {
            var friends1 = await GetActiveFriendIdsAsync(userId1);
            var friends2 = await GetActiveFriendIdsAsync(userId2);
            return friends1.Intersect(friends2).Count();
        }
    }
}

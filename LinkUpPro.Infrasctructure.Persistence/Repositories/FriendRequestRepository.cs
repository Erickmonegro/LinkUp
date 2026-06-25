using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using LinkUpPro.Core.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class FriendRequestRepository : GenericRepository<FriendRequest>, IFriendRequestRepository
    {
        private readonly LinkUpAppContext repo;
        public FriendRequestRepository(LinkUpAppContext context) : base(context)
        {
            repo = context;
        }

        public async Task<List<FriendRequest>> GetPendingRequestsReceivedAsync(string userId)
        {
            return await repo.Set<FriendRequest>()
                .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                .ToListAsync();
        }

        public async Task<FriendRequest?> GetRequestBetweenUsersAsync(string user1Id, string user2Id)
        {
            return await repo.Set<FriendRequest>()
                .FirstOrDefaultAsync(fr => 
                    (fr.SenderId == user1Id && fr.ReceiverId == user2Id) || 
                    (fr.SenderId == user2Id && fr.ReceiverId == user1Id));
        }

        public async Task<List<FriendRequest>> GetSentRequestsAsync(string userId)
        {
            return await repo.Set<FriendRequest>()
                .Where(fr => fr.SenderId == userId)
                .ToListAsync();
        }

        public async Task<bool> HasPendingRequestAsync(string senderId, string receiverId)
        {
            return await repo.Set<FriendRequest>()
                .AnyAsync(fr => 
                    ((fr.SenderId == senderId && fr.ReceiverId == receiverId) || 
                     (fr.SenderId == receiverId && fr.ReceiverId == senderId)) &&
                    fr.Status == FriendRequestStatus.Pending);
        }
    }
}

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
                .Where(fr => fr.SenderId == userId && !fr.IsHiddenForSender)
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

        public async Task<bool> AcceptRequestAtomicAsync(int requestId, string receiverId)
        {
            var request = await repo.Set<FriendRequest>().FirstOrDefaultAsync(fr => fr.Id == requestId);
            if (request == null || request.ReceiverId != receiverId || request.Status != FriendRequestStatus.Pending)
            {
                return false;
            }

            request.Status = FriendRequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;

            var existingFriendship = await repo.Set<Friendship>().FirstOrDefaultAsync(f => 
                (f.UserId1 == request.SenderId && f.UserId2 == request.ReceiverId) ||
                (f.UserId1 == request.ReceiverId && f.UserId2 == request.SenderId));

            if (existingFriendship != null)
            {
                existingFriendship.Status = FriendshipStatus.Active;
            }
            else
            {
                // Canonical ordering to enforce consistency and prevent reversed duplicates
                string u1 = string.Compare(request.SenderId, request.ReceiverId, StringComparison.Ordinal) < 0 ? request.SenderId : request.ReceiverId;
                string u2 = string.Compare(request.SenderId, request.ReceiverId, StringComparison.Ordinal) < 0 ? request.ReceiverId : request.SenderId;

                await repo.Set<Friendship>().AddAsync(new Friendship
                {
                    Id = 0,
                    UserId1 = u1,
                    UserId2 = u2,
                    Status = FriendshipStatus.Active
                });
            }

            await repo.SaveChangesAsync();
            return true;
        }
    }
}

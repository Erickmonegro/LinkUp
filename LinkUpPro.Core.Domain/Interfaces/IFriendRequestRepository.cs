using LinkUpPro.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IFriendRequestRepository : IGenericRepository<FriendRequest>
    {
        Task<bool> HasPendingRequestAsync(string senderId, string receiverId);
        Task<List<FriendRequest>> GetPendingRequestsReceivedAsync(string userId);
        Task<List<FriendRequest>> GetSentRequestsAsync(string userId);
        Task<FriendRequest?> GetRequestBetweenUsersAsync(string user1Id, string user2Id);
        Task<bool> AcceptRequestAtomicAsync(int requestId, string receiverId);
    }
}

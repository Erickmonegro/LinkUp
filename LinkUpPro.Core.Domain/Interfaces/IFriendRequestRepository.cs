using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IFriendRequestRepository : IGenericRepository<FriendRequest>
    {
        Task<bool> HasPendingRequestAsync(string senderId, string receiverId);
        Task<List<FriendRequest>> GetPendingRequestsReceivedAsync(string userId);
        Task<List<FriendRequest>> GetSentRequestsAsync(string userId);
        Task<FriendRequest?> GetRequestBetweenUsersAsync(string user1Id, string user2Id);
    }
}

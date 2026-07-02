using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IFriendshipRepository : IGenericRepository<Friendship>
    {
        Task<List<string>> GetActiveFriendIdsAsync(string userId);
        Task<int> GetMutualFriendsCountAsync(string userId1, string userId2);
        Task<List<string>> GetMutualFriendIdsAsync(string userId1, string userId2);
        Task<bool> AreFriendsAsync(string userId1, string userId2);
    }
}

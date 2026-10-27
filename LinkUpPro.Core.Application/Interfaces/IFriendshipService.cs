using LinkUpPro.Core.Application.DTOs.FriendshipDTO;
using LinkUpPro.Core.Application.DTOs.PostDTO;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IFriendshipService : IGenericService<FriendshipDTO>
    {
        Task<List<FriendshipDTO>> GetFriendsAsync(string userId);
        Task<int> GetActiveFriendsCountAsync(string userId);
        Task<int> GetFriendPostsCountAsync(string userId);
        Task<List<PostDTO>> GetFriendPostsAsync(string userId, string? textFilter = null, string? friendIdFilter = null, int? contentTypeFilter = null, DateTime? dateFrom = null, DateTime? dateTo = null, int? editionStatusFilter = null);
        Task<UserDTO?> GetFriendProfileAsync(string userId, string friendId);
        Task<List<UserDTO>> GetMutualFriendsAsync(string userId1, string userId2);
        Task<bool> DeleteFriendshipAsync(string userId, int friendshipId);
        Task<bool> AreFriendsAsync(string userId1, string userId2);
    }
}

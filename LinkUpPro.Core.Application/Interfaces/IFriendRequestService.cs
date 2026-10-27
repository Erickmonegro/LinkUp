using LinkUpPro.Core.Application.DTOs.FriendRequestDTO;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IFriendRequestService : IGenericService<FriendRequestDTO>
    {
        Task<List<FriendRequestDTO>> GetPendingReceivedAsync(string userId);
        Task<List<FriendRequestDTO>> GetSentRequestsAsync(string userId);
        Task<int> GetPendingCountAsync(string userId);
        Task<bool> AcceptRequestAsync(string userId, int requestId);
        Task<bool> RejectRequestAsync(string userId, int requestId);
        Task<bool> CancelRequestAsync(string userId, int requestId);
        Task<bool> HideRequestAsync(string userId, int requestId);
        Task<bool> SendRequestAsync(string userId, string receiverId);
        Task<List<UserDTO>> SearchAvailableUsersAsync(string userId, string searchTerm);
    }
}

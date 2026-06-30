using AutoMapper;
using LinkUpPro.Core.Application.DTOs.FriendRequestDTO;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace LinkUpPro.Core.Application.Services
{
    public class FriendRequestService : GenericService<FriendRequest, FriendRequestDTO>, IFriendRequestService
    {
        private readonly IFriendRequestRepository _repository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public FriendRequestService(
            IFriendRequestRepository repository, 
            IFriendshipRepository friendshipRepository,
            IUserService userService, 
            IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _friendshipRepository = friendshipRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<bool> AcceptRequestAsync(string userId, int requestId)
        {
            var request = await _repository.GetByIdAsync(requestId);
            if (request == null || request.ReceiverId != userId || request.Status != LinkUpPro.Core.Domain.Common.Enums.FriendRequestStatus.Pending)
                return false;

            request.Status = LinkUpPro.Core.Domain.Common.Enums.FriendRequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(requestId, request);

            await _friendshipRepository.AddAsync(new Friendship
            {
                Id = 0,
                UserId1 = request.SenderId,
                UserId2 = request.ReceiverId
            });

            return true;
        }

        public async Task<bool> CancelRequestAsync(string userId, int requestId)
        {
            var request = await _repository.GetByIdAsync(requestId);
            if (request == null || request.SenderId != userId || request.Status != LinkUpPro.Core.Domain.Common.Enums.FriendRequestStatus.Pending)
                return false;

            await _repository.DeleteAsync(requestId);
            return true;
        }

        public async Task<int> GetPendingCountAsync(string userId)
        {
            var pending = await _repository.GetPendingRequestsReceivedAsync(userId);
            return pending.Count;
        }

        public async Task<List<FriendRequestDTO>> GetPendingReceivedAsync(string userId)
        {
            var pending = await _repository.GetPendingRequestsReceivedAsync(userId);
            return _mapper.Map<List<FriendRequestDTO>>(pending);
        }

        public async Task<List<FriendRequestDTO>> GetSentRequestsAsync(string userId)
        {
            var sent = await _repository.GetSentRequestsAsync(userId);
            return _mapper.Map<List<FriendRequestDTO>>(sent);
        }

        public async Task<bool> HideRequestAsync(string userId, int requestId)
        {
            var request = await _repository.GetByIdAsync(requestId);
            if (request == null) return false;

            if (request.SenderId == userId) request.IsHiddenForSender = true;
            else if (request.ReceiverId == userId) request.IsHiddenForReceiver = true;
            else return false;

            await _repository.UpdateAsync(requestId, request);
            return true;
        }

        public async Task<bool> RejectRequestAsync(string userId, int requestId)
        {
            var request = await _repository.GetByIdAsync(requestId);
            if (request == null || request.ReceiverId != userId || request.Status != LinkUpPro.Core.Domain.Common.Enums.FriendRequestStatus.Pending)
                return false;

            request.Status = LinkUpPro.Core.Domain.Common.Enums.FriendRequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(requestId, request);
            return true;
        }

        public async Task<List<UserDTO>> SearchAvailableUsersAsync(string userId, string searchTerm)
        {
            var allUsers = await _userService.GetAllUser(true);
            var friends = await _friendshipRepository.GetActiveFriendIdsAsync(userId);
            
            var available = allUsers.Where(u => u.Id != userId && !friends.Contains(u.Id));
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                available = available.Where(u => u.FirstName.ToLower().Contains(searchTerm) || u.LastName.ToLower().Contains(searchTerm));
            }
            
            return available.ToList();
        }

        public async Task<bool> SendRequestAsync(string userId, string receiverId)
        {
            if (userId == receiverId) return false;

            var areFriends = await _friendshipRepository.AreFriendsAsync(userId, receiverId);
            if (areFriends) return false;

            var hasPending = await _repository.HasPendingRequestAsync(userId, receiverId);
            if (hasPending) return false;

            await _repository.AddAsync(new FriendRequest
            {
                Id = 0,
                SenderId = userId,
                ReceiverId = receiverId,
                Status = LinkUpPro.Core.Domain.Common.Enums.FriendRequestStatus.Pending
            });

            return true;
        }
    }
}

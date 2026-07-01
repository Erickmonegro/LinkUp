using AutoMapper;
using LinkUpPro.Core.Application.DTOs.FriendRequestDTO;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Core.Domain.Common.Enums;
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
            if (request == null || request.ReceiverId != userId || request.Status != FriendRequestStatus.Pending)
                return false;

            request.Status = FriendRequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(requestId, request);

            // Reactivate friendship if it exists, or create new one
            var existingFriendships = await _friendshipRepository.GetAllListAsync();
            var existingFriendship = existingFriendships.FirstOrDefault(f => 
                (f.UserId1 == request.SenderId && f.UserId2 == request.ReceiverId) ||
                (f.UserId1 == request.ReceiverId && f.UserId2 == request.SenderId));

            if (existingFriendship != null)
            {
                existingFriendship.Status = FriendshipStatus.Active;
                await _friendshipRepository.UpdateAsync(existingFriendship.Id, existingFriendship);
            }
            else
            {
                await _friendshipRepository.AddAsync(new Friendship
                {
                    Id = 0,
                    UserId1 = request.SenderId,
                    UserId2 = request.ReceiverId,
                    Status = FriendshipStatus.Active
                });
            }

            return true;
        }

        public async Task<bool> CancelRequestAsync(string userId, int requestId)
        {
            var request = await _repository.GetByIdAsync(requestId);
            if (request == null || request.SenderId != userId || request.Status != FriendRequestStatus.Pending)
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
            var dtos = _mapper.Map<List<FriendRequestDTO>>(pending);
            
            foreach (var dto in dtos)
            {
                var sender = await _userService.GetUserById(dto.SenderId);
                if (sender != null)
                {
                    dto.SenderName = $"{sender.FirstName} {sender.LastName}";
                    dto.SenderPhotoUrl = sender.PhotoUrl;
                }
                else
                {
                    dto.SenderName = "Usuario";
                }
                
                var receiver = await _userService.GetUserById(dto.ReceiverId);
                if (receiver != null)
                {
                    dto.ReceiverName = $"{receiver.FirstName} {receiver.LastName}";
                    dto.ReceiverPhotoUrl = receiver.PhotoUrl;
                }
                else
                {
                    dto.ReceiverName = "Usuario";
                }
                
                dto.MutualFriendsCount = await _friendshipRepository.GetMutualFriendsCountAsync(dto.SenderId, dto.ReceiverId);
            }
            
            return dtos;
        }

        public async Task<List<FriendRequestDTO>> GetSentRequestsAsync(string userId)
        {
            var sent = await _repository.GetSentRequestsAsync(userId);
            var dtos = _mapper.Map<List<FriendRequestDTO>>(sent);
            
            foreach (var dto in dtos)
            {
                var sender = await _userService.GetUserById(dto.SenderId);
                if (sender != null)
                {
                    dto.SenderName = $"{sender.FirstName} {sender.LastName}";
                    dto.SenderPhotoUrl = sender.PhotoUrl;
                }
                else
                {
                    dto.SenderName = "Usuario";
                }
                
                var receiver = await _userService.GetUserById(dto.ReceiverId);
                if (receiver != null)
                {
                    dto.ReceiverName = $"{receiver.FirstName} {receiver.LastName}";
                    dto.ReceiverPhotoUrl = receiver.PhotoUrl;
                }
                else
                {
                    dto.ReceiverName = "Usuario";
                }
                
                dto.MutualFriendsCount = await _friendshipRepository.GetMutualFriendsCountAsync(dto.SenderId, dto.ReceiverId);
            }
            
            return dtos;
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
            if (request == null || request.ReceiverId != userId || request.Status != FriendRequestStatus.Pending)
                return false;

            request.Status = FriendRequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(requestId, request);
            return true;
        }

        public async Task<List<UserDTO>> SearchAvailableUsersAsync(string userId, string searchTerm)
        {
            var allUsers = await _userService.GetAllUser(true);
            var friends = await _friendshipRepository.GetActiveFriendIdsAsync(userId);
            
            // Exclude self and current active friends
            var available = allUsers.Where(u => u.Id != userId && !friends.Contains(u.Id));
            
            // Exclude users where there is a pending request in either direction
            var pendingRequests = await _repository.GetAllListAsync();
            var excludedUserIds = pendingRequests
                .Where(r => r.Status == FriendRequestStatus.Pending && (r.SenderId == userId || r.ReceiverId == userId))
                .Select(r => r.SenderId == userId ? r.ReceiverId : r.SenderId)
                .ToList();

            available = available.Where(u => !excludedUserIds.Contains(u.Id));

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                available = available.Where(u => 
                    u.FirstName.ToLower().Contains(searchTerm) || 
                    u.LastName.ToLower().Contains(searchTerm) ||
                    u.UserName.ToLower().Contains(searchTerm));
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
                Status = FriendRequestStatus.Pending
            });

            return true;
        }
    }
}

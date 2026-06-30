using AutoMapper;
using LinkUpPro.Core.Application.DTOs.FriendshipDTO;
using LinkUpPro.Core.Application.DTOs.PostDTO;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Services
{
    public class FriendshipService : GenericService<Friendship, FriendshipDTO>, IFriendshipService
    {
        private readonly IFriendshipRepository _repository;
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public FriendshipService(IFriendshipRepository repository, IPostRepository postRepository, IUserService userService, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _postRepository = postRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<bool> DeleteFriendshipAsync(string userId, int friendshipId)
        {
            var friendship = await _repository.GetByIdAsync(friendshipId);
            if (friendship == null) return false;
            
            if (friendship.UserId1 != userId && friendship.UserId2 != userId)
                return false;

            await _repository.DeleteAsync(friendshipId);
            return true;
        }

        public async Task<int> GetActiveFriendsCountAsync(string userId)
        {
            var friends = await _repository.GetActiveFriendIdsAsync(userId);
            return friends.Count;
        }

        public async Task<List<PostDTO>> GetFriendPostsAsync(string userId, string? textFilter = null, string? friendIdFilter = null, int? contentTypeFilter = null, DateTime? dateFrom = null, DateTime? dateTo = null, int? editionStatusFilter = null)
        {
            var friendIds = await _repository.GetActiveFriendIdsAsync(userId);
            if (!friendIds.Any()) return new List<PostDTO>();

            if (!string.IsNullOrEmpty(friendIdFilter))
            {
                if (!friendIds.Contains(friendIdFilter)) return new List<PostDTO>();
                friendIds = new List<string> { friendIdFilter };
            }

            var posts = await _postRepository.GetFriendsPostsAsync(friendIds, textFilter, contentTypeFilter, dateFrom, dateTo, editionStatusFilter);
            return _mapper.Map<List<PostDTO>>(posts);
        }

        public async Task<int> GetFriendPostsCountAsync(string userId)
        {
            var friendIds = await _repository.GetActiveFriendIdsAsync(userId);
            if (!friendIds.Any()) return 0;
            return await _postRepository.GetFriendsPostsCountAsync(friendIds);
        }

        public async Task<UserDTO?> GetFriendProfileAsync(string userId, string friendId)
        {
            bool areFriends = await _repository.AreFriendsAsync(userId, friendId);
            if (!areFriends) return null;

            return await _userService.GetUserById(friendId);
        }

        public async Task<List<FriendshipDTO>> GetFriendsAsync(string userId)
        {
            var allFriendships = await _repository.GetAllListAsync();
            var userFriendships = allFriendships
                .Where(f => f.UserId1 == userId || f.UserId2 == userId)
                .ToList();
                
            return _mapper.Map<List<FriendshipDTO>>(userFriendships);
        }
    }
}

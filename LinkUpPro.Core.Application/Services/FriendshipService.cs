using AutoMapper;
using LinkUpPro.Core.Application.DTOs.FriendshipDTO;
using LinkUpPro.Core.Application.DTOs.PostDTO;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Core.Domain.Common.Enums;
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

            // Logical deletion per PDF requirements: "cambiando el estado de la relación a eliminada."
            friendship.Status = FriendshipStatus.Deleted;
            await _repository.UpdateAsync(friendshipId, friendship);
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
            var dtos = _mapper.Map<List<PostDTO>>(posts);
            
            // Populate author info for posts of friends
            var users = new Dictionary<string, UserDTO>();
            foreach (var fId in friendIds)
            {
                var user = await _userService.GetUserById(fId);
                if (user != null) users[fId] = user;
            }

            foreach (var post in dtos)
            {
                if (users.TryGetValue(post.AuthorId, out var user))
                {
                    post.AuthorName = $"{user.FirstName} {user.LastName}";
                    post.AuthorPhotoUrl = user.PhotoUrl;
                }
            }

            return dtos;
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
                .Where(f => (f.UserId1 == userId || f.UserId2 == userId) && f.Status == FriendshipStatus.Active)
                .ToList();
                
            var dtos = _mapper.Map<List<FriendshipDTO>>(userFriendships);

            foreach (var dto in dtos)
            {
                dto.FriendId = dto.UserId1 == userId ? dto.UserId2 : dto.UserId1;
                
                var friendUser = await _userService.GetUserById(dto.FriendId);
                if (friendUser != null)
                {
                    dto.FriendName = $"{friendUser.FirstName} {friendUser.LastName}";
                    dto.FriendPhotoUrl = friendUser.PhotoUrl;
                }
                else
                {
                    dto.FriendName = "Usuario Inactivo";
                }
                
                dto.MutualFriendsCount = await _repository.GetMutualFriendsCountAsync(userId, dto.FriendId);
            }
            
            // Sort alphabetically by friend name as required by PDF
            return dtos.OrderBy(f => f.FriendName).ToList();
        }
    }
}

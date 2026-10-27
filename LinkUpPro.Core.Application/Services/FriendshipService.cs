using AutoMapper;
using LinkUpPro.Core.Application.DTOs.FriendshipDTO;
using LinkUpPro.Core.Application.DTOs.PostDTO;
using LinkUpPro.Core.Application.DTOs.CommentDTO;
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

            friendship.Status = FriendshipStatus.Deleted;
            await _repository.UpdateAsync(friendshipId, friendship);
            return true;
        }

        public async Task<bool> AreFriendsAsync(string userId1, string userId2)
        {
            return await _repository.AreFriendsAsync(userId1, userId2);
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
                    post.AuthorUserName = user.UserName;
                    post.AuthorPhotoUrl = user.PhotoUrl;
                }

                if (post.Comments != null && post.Comments.Count > 0)
                {
                    await PopulateCommentAuthorsAsync(post.Comments.ToList());
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
                    dto.FriendUserName = friendUser.UserName;
                    dto.FriendPhotoUrl = friendUser.PhotoUrl;
                }
                else
                {
                    dto.FriendName = "Usuario Inactivo";
                }
                
                dto.MutualFriendsCount = await _repository.GetMutualFriendsCountAsync(userId, dto.FriendId);
            }
            
            return dtos.OrderBy(f => f.FriendName).ToList();
        }

        public async Task<List<UserDTO>> GetMutualFriendsAsync(string userId1, string userId2)
        {
            var mutualIds = await _repository.GetMutualFriendIdsAsync(userId1, userId2);
            var result = new List<UserDTO>();
            foreach (var id in mutualIds)
            {
                var user = await _userService.GetUserById(id);
                if (user != null)
                {
                    result.Add(user);
                }
            }
            return result.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToList();
        }

        #region Private Helpers for Comments
        private async Task PopulateCommentAuthorsAsync(List<CommentDTO> commentDtos)
        {
            if (commentDtos == null || commentDtos.Count == 0) return;

            var authorIds = new List<string>();
            GetCommentAuthorIds(commentDtos, authorIds);
            authorIds = authorIds.Distinct().ToList();

            var users = new Dictionary<string, UserDTO>();
            foreach (var authorId in authorIds)
            {
                var user = await _userService.GetUserById(authorId);
                if (user != null)
                {
                    users[authorId] = user;
                }
            }

            DecorateComments(commentDtos, users);
        }

        private void GetCommentAuthorIds(ICollection<CommentDTO> comments, List<string> ids)
        {
            foreach (var comment in comments)
            {
                ids.Add(comment.AuthorId);
                if (comment.Replies != null && comment.Replies.Count > 0)
                {
                    GetCommentAuthorIds(comment.Replies, ids);
                }
            }
        }

        private void DecorateComments(ICollection<CommentDTO> comments, Dictionary<string, UserDTO> users)
        {
            foreach (var comment in comments)
            {
                if (users.TryGetValue(comment.AuthorId, out var user))
                {
                    comment.AuthorName = $"{user.FirstName} {user.LastName}";
                    comment.AuthorUserName = user.UserName;
                    comment.AuthorPhotoUrl = user.PhotoUrl;
                }

                if (comment.Replies != null && comment.Replies.Count > 0)
                {
                    DecorateComments(comment.Replies, users);
                }
            }
        }
        #endregion
    }
}

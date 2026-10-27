using AutoMapper;
using LinkUpPro.Core.Application.DTOs.CommentDTO;
using LinkUpPro.Core.Application.DTOs.PostDTO;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Common.Enums;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Services
{
    public class PostService : GenericService<Post, PostDTO>, IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public PostService(IPostRepository postRepository, IMapper mapper, IUserService userService) : base(postRepository, mapper)
        {
            _postRepository = postRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public override async Task<PostDTO?> GetByIdAsync(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null) return null;

            var dto = _mapper.Map<PostDTO>(post);
            await PopulateAuthorsAsync(new List<PostDTO> { dto });
            return dto;
        }

        public async Task<List<PostDTO>> GetHomePostsAsync(string userId)
        {
            var posts = await _postRepository.GetFeedAsync(userId);
            var dtos = _mapper.Map<List<PostDTO>>(posts);
            await PopulateAuthorsAsync(dtos);
            return dtos;
        }

        public async Task<List<PostDTO>> GetHomePostsFilteredAsync(string userId, string? textFilter, ContentType? contentType, DateTime? dateFrom, DateTime? dateTo, EditionStatus? editionStatus)
        {
            var posts = await _postRepository.GetFeedFilteredAsync(
                userId,
                textFilter,
                contentType.HasValue ? (int)contentType.Value : null,
                dateFrom,
                dateTo,
                editionStatus.HasValue ? (int)editionStatus.Value : null
            );
            var dtos = _mapper.Map<List<PostDTO>>(posts);
            await PopulateAuthorsAsync(dtos);
            return dtos;
        }

        public async Task<List<PostDTO>> GetUserPostsFilteredAsync(string userId, string? textFilter, ContentType? contentType, DateTime? dateFrom, DateTime? dateTo, EditionStatus? editionStatus)
        {
            var posts = await _postRepository.GetUserPostsAsync(userId);
            var query = posts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(textFilter))
            {
                var cleanFilter = textFilter.Trim().ToLower();
                query = query.Where(p => p.TextContent.ToLower().Contains(cleanFilter));
            }

            if (contentType.HasValue && contentType.Value != ContentType.All)
            {
                query = query.Where(p => p.ContentType == contentType.Value);
            }

            if (dateFrom.HasValue)
            {
                var from = dateFrom.Value.Date;
                query = query.Where(p => p.CreatedAt >= from);
            }

            if (dateTo.HasValue)
            {
                var to = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.CreatedAt <= to);
            }

            if (editionStatus.HasValue && editionStatus.Value != EditionStatus.All)
            {
                query = query.Where(p => p.EditionStatus == editionStatus.Value);
            }

            var filteredPosts = query.ToList();
            var dtos = _mapper.Map<List<PostDTO>>(filteredPosts);
            await PopulateAuthorsAsync(dtos);
            return dtos;
        }

        public async Task<bool> ToggleCommentsAsync(int postId, string userId, bool allowComments)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null || post.AuthorId != userId) return false;

            post.CommentStatus = allowComments 
                ? CommentStatus.Allowed 
                : CommentStatus.NotAllowed;
                
            await _postRepository.UpdateAsync(postId, post);
            return true;
        }

        #region Private Helpers

        private async Task PopulateAuthorsAsync(List<PostDTO> postDtos)
        {
            if (postDtos == null || postDtos.Count == 0) return;

            var authorIds = postDtos.Select(p => p.AuthorId).Distinct().ToList();
            var users = new Dictionary<string, UserDTO>();
            
            foreach (var authorId in authorIds)
            {
                var user = await _userService.GetUserById(authorId);
                if (user != null)
                {
                    users[authorId] = user;
                }
            }

            foreach (var post in postDtos)
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
        }

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

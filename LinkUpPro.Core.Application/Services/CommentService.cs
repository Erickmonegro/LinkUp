using AutoMapper;
using LinkUpPro.Core.Application.DTOs.CommentDTO;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Services
{
    public class CommentService : GenericService<Comment, CommentDTO>, ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository commentRepository, IMapper mapper, IUserService userService) : base(commentRepository, mapper)
        {
            _commentRepository = commentRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<List<CommentDTO>> GetCommentsByPostAsync(int postId)
        {
            var comments = await _commentRepository.GetCommentsByPostAsync(postId);
            var dtos = _mapper.Map<List<CommentDTO>>(comments);
            await PopulateCommentAuthorsAsync(dtos);
            return dtos;
        }

        public async Task<CommentDTO> ReplyToCommentAsync(int parentCommentId, CommentDTO reply)
        {
            var entity = _mapper.Map<Comment>(reply);
            entity.ParentCommentId = parentCommentId;
            var result = await _commentRepository.AddAsync(entity);
            
            var dto = _mapper.Map<CommentDTO>(result);
            await PopulateCommentAuthorsAsync(new List<CommentDTO> { dto });
            return dto;
        }

        #region Private Helpers

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

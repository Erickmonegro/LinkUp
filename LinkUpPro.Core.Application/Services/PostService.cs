using AutoMapper;
using LinkUpPro.Core.Application.DTOs.PostDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Services
{
    public class PostService : GenericService<Post, PostDTO>, IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public PostService(IPostRepository postRepository, IMapper mapper) : base(postRepository, mapper)
        {
            _postRepository = postRepository;
            _mapper = mapper;
        }

        public async Task<List<PostDTO>> GetHomePostsAsync(string userId)
        {
            var posts = await _postRepository.GetFeedAsync(userId);
            return _mapper.Map<List<PostDTO>>(posts);
        }

        public async Task<bool> ToggleCommentsAsync(int postId, string userId, bool allowComments)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null || post.AuthorId != userId) return false;

            post.CommentStatus = allowComments 
                ? LinkUpPro.Core.Domain.Common.Enums.CommentStatus.Allowed 
                : LinkUpPro.Core.Domain.Common.Enums.CommentStatus.NotAllowed;
                
            await _postRepository.UpdateAsync(postId, post);
            return true;
        }
    }
}

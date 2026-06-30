using AutoMapper;
using LinkUpPro.Core.Application.DTOs.CommentDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Services
{
    public class CommentService : GenericService<Comment, CommentDTO>, ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository commentRepository, IMapper mapper) : base(commentRepository, mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        public async Task<List<CommentDTO>> GetCommentsByPostAsync(int postId)
        {
            var comments = await _commentRepository.GetCommentsByPostAsync(postId);
            return _mapper.Map<List<CommentDTO>>(comments);
        }

        public async Task<CommentDTO> ReplyToCommentAsync(int parentCommentId, CommentDTO reply)
        {
            var entity = _mapper.Map<Comment>(reply);
            entity.ParentCommentId = parentCommentId;
            var result = await _commentRepository.AddAsync(entity);
            return _mapper.Map<CommentDTO>(result);
        }
    }
}

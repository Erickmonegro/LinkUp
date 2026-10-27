using LinkUpPro.Core.Application.DTOs.CommentDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface ICommentService : IGenericService<CommentDTO>
    {
        Task<List<CommentDTO>> GetCommentsByPostAsync(int postId);
        Task<CommentDTO> ReplyToCommentAsync(int parentCommentId, CommentDTO reply);
        Task<bool> HasRepliesAsync(int commentId);
    }
}

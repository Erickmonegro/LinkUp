using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        Task<List<Comment>> GetCommentsByPostAsync(int postId);
        Task<bool> HasRepliesAsync(int commentId);
        Task<Comment?> GetCommentWithRepliesAsync(int commentId);
    }
}

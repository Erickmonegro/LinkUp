using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<List<Post>> GetFeedAsync(string userId);
        Task<List<Post>> GetUserPostsAsync(string userId);
    }
}

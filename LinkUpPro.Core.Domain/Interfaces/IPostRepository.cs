using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<List<Post>> GetFeedAsync(string userId);
        Task<List<Post>> GetFeedFilteredAsync(string userId, string? textFilter = null, int? contentTypeFilter = null, DateTime? dateFrom = null, DateTime? dateTo = null, int? editionStatusFilter = null);
        Task<List<Post>> GetUserPostsAsync(string userId);
        Task<List<Post>> GetFriendsPostsAsync(List<string> friendIds, string? textFilter = null, int? contentTypeFilter = null, DateTime? dateFrom = null, DateTime? dateTo = null, int? editionStatusFilter = null);
        Task<int> GetFriendsPostsCountAsync(List<string> friendIds);
    }
}

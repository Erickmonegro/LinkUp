using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IReactionRepository : IGenericRepository<Reaction>
    {
        Task<Reaction?> GetReactionByUserAndPostAsync(string userId, int postId);
    }
}

using LinkUpPro.Core.Application.DTOs.ReactionDTO;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IReactionService : IGenericService<ReactionDTO>
    {
        Task<bool> ToggleReactionAsync(int postId, string userId, bool isLike);
        Task<bool> DeleteReactionAsync(int postId, string userId);
        Task<(int Likes, int Dislikes)> GetReactionCountsAsync(int postId);
    }
}

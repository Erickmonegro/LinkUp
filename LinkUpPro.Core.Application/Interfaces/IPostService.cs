using LinkUpPro.Core.Application.DTOs.PostDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IPostService : IGenericService<PostDTO>
    {
        Task<List<PostDTO>> GetHomePostsAsync(string userId);
        Task<bool> ToggleCommentsAsync(int postId, string userId, bool allowComments);
    }
}

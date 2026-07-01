using LinkUpPro.Core.Application.DTOs.PostDTO;
using LinkUpPro.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IPostService : IGenericService<PostDTO>
    {
        Task<List<PostDTO>> GetHomePostsAsync(string userId);
        Task<List<PostDTO>> GetUserPostsFilteredAsync(string userId, string? textFilter, ContentType? contentType, DateTime? dateFrom, DateTime? dateTo, EditionStatus? editionStatus);
        Task<bool> ToggleCommentsAsync(int postId, string userId, bool allowComments);
    }
}

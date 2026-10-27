using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        private readonly LinkUpAppContext repo;
        public PostRepository(LinkUpAppContext context) : base(context)
        {
            repo = context;
        }

        public async Task<List<Post>> GetFeedAsync(string userId)
        {
            return await repo.Set<Post>()
                .Where(p => p.IsActive && p.AuthorId == userId)
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetFeedFilteredAsync(string userId, string? textFilter = null, int? contentTypeFilter = null, DateTime? dateFrom = null, DateTime? dateTo = null, int? editionStatusFilter = null)
        {
            var query = repo.Set<Post>().AsQueryable();

            query = query.Where(p => p.IsActive && p.AuthorId == userId);

            if (!string.IsNullOrWhiteSpace(textFilter))
            {
                query = query.Where(p => p.TextContent.Contains(textFilter));
            }

            if (contentTypeFilter.HasValue && contentTypeFilter.Value != (int)LinkUpPro.Core.Domain.Common.Enums.ContentType.All)
            {
                query = query.Where(p => (int)p.ContentType == contentTypeFilter.Value);
            }

            if (dateFrom.HasValue)
            {
                var from = dateFrom.Value.Date;
                query = query.Where(p => p.CreatedAt >= from);
            }

            if (dateTo.HasValue)
            {
                var to = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.CreatedAt <= to);
            }

            if (editionStatusFilter.HasValue && editionStatusFilter.Value != (int)LinkUpPro.Core.Domain.Common.Enums.EditionStatus.All)
            {
                query = query.Where(p => (int)p.EditionStatus == editionStatusFilter.Value);
            }

            return await query
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetUserPostsAsync(string userId)
        {
            return await repo.Set<Post>()
                .Where(p => p.AuthorId == userId && p.IsActive)
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetFriendsPostsAsync(List<string> friendIds, string? textFilter = null, int? contentTypeFilter = null, DateTime? dateFrom = null, DateTime? dateTo = null, int? editionStatusFilter = null)
        {
            var query = repo.Set<Post>().AsQueryable();

            query = query.Where(p => friendIds.Contains(p.AuthorId) && p.IsActive && p.PrivacyMode != LinkUpPro.Core.Domain.Common.Enums.PrivacyMode.OnlyMe);

            if (!string.IsNullOrWhiteSpace(textFilter))
            {
                query = query.Where(p => p.TextContent.Contains(textFilter));
            }

            if (contentTypeFilter.HasValue && contentTypeFilter.Value != (int)LinkUpPro.Core.Domain.Common.Enums.ContentType.All)
            {
                query = query.Where(p => (int)p.ContentType == contentTypeFilter.Value);
            }

            if (dateFrom.HasValue)
            {
                var from = dateFrom.Value.Date;
                query = query.Where(p => p.CreatedAt >= from);
            }

            if (dateTo.HasValue)
            {
                var to = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.CreatedAt <= to);
            }

            if (editionStatusFilter.HasValue && editionStatusFilter.Value != (int)LinkUpPro.Core.Domain.Common.Enums.EditionStatus.All)
            {
                query = query.Where(p => (int)p.EditionStatus == editionStatusFilter.Value);
            }

            return await query
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetFriendsPostsCountAsync(List<string> friendIds)
        {
            return await repo.Set<Post>()
                .CountAsync(p => friendIds.Contains(p.AuthorId) && p.IsActive && p.PrivacyMode != LinkUpPro.Core.Domain.Common.Enums.PrivacyMode.OnlyMe);
        }
    }
}

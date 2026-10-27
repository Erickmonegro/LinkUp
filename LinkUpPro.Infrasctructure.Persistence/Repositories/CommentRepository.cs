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
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        private readonly LinkUpAppContext repo;
        public CommentRepository(LinkUpAppContext context) : base(context)
        {
           repo = context;
        }

        public async Task<List<Comment>> GetCommentsByPostAsync(int postId)
        {
            return await repo.Set<Comment>()
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .Include(c => c.Replies)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentWithRepliesAsync(int commentId)
        {
            return await repo.Set<Comment>()
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<bool> HasRepliesAsync(int commentId)
        {
            return await repo.Set<Comment>()
                .AnyAsync(c => c.ParentCommentId == commentId);
        }

        public override async Task<Comment?> UpdateAsync(int id, Comment entity)
        {
            var existingComment = await repo.Set<Comment>().FindAsync(id);
            if (existingComment != null)
            {
                existingComment.Content = entity.Content;
                existingComment.UpdatedAt = DateTime.UtcNow;
                await repo.SaveChangesAsync();
                return existingComment;
            }
            return null;
        }
    }
}

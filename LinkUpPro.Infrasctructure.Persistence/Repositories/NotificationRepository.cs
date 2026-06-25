using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using LinkUpPro.Core.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly LinkUpAppContext repo;
        public NotificationRepository(LinkUpAppContext context) : base(context)
        {
            repo = context;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await repo.Set<Notification>()
                .CountAsync(n => n.ReceiverId == userId && n.Status == NotificationStatus.Unread);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await repo.Set<Notification>()
                .Where(n => n.ReceiverId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unread = await repo.Set<Notification>()
                .Where(n => n.ReceiverId == userId && n.Status == NotificationStatus.Unread)
                .ToListAsync();

            foreach (var notification in unread)
            {
                notification.Status = NotificationStatus.Read;
            }
            
            await repo.SaveChangesAsync();
        }
    }
}   


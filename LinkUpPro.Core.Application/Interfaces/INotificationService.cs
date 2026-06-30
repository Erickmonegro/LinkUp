using LinkUpPro.Core.Application.DTOs.NotificationDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface INotificationService : IGenericService<NotificationDTO>
    {
        Task<List<NotificationDTO>> GetUserNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
    }
}

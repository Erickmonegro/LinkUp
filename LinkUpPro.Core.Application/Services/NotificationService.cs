using AutoMapper;
using LinkUpPro.Core.Application.DTOs.NotificationDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Services
{
    public class NotificationService : GenericService<Notification, NotificationDTO>, INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _repository.GetUnreadCountAsync(userId);
        }

        public async Task<List<NotificationDTO>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _repository.GetUserNotificationsAsync(userId);
            return _mapper.Map<List<NotificationDTO>>(notifications);
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            await _repository.MarkAllAsReadAsync(userId);
            return true;
        }
    }
}

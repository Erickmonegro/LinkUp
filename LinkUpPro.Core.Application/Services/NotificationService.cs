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
        private readonly IUserService _userService;

        public NotificationService(INotificationRepository repository, IMapper mapper, IUserService userService) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _repository.GetUnreadCountAsync(userId);
        }

        public async Task<List<NotificationDTO>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _repository.GetUserNotificationsAsync(userId);
            var dtos = _mapper.Map<List<NotificationDTO>>(notifications);
            foreach (var dto in dtos)
            {
                if (!string.IsNullOrEmpty(dto.TriggerUserId))
                {
                    var user = await _userService.GetUserById(dto.TriggerUserId);
                    if (user != null)
                    {
                        dto.TriggerUserPhotoUrl = user.PhotoUrl;
                        if (string.IsNullOrEmpty(dto.TriggerUserName) || dto.TriggerUserName == "Un usuario")
                        {
                            dto.TriggerUserName = user.UserName;
                        }
                    }
                }
            }
            return dtos;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            await _repository.MarkAllAsReadAsync(userId);
            return true;
        }
    }
}

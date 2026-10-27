using AutoMapper;
using LinkUpPro.Core.Application.DTOs.NotificationDTO;
using LinkUpPro.Core.Application.ViewModels.Notification;
using LinkUpPro.Core.Domain.Common.Enums;

namespace LinkUpPro.Core.Application.Mappings.DtosAndViewModels
{
    public class NotificationDtoMappingProfile : Profile
    {
        public NotificationDtoMappingProfile()
        {
            CreateMap<NotificationDTO, NotificationViewModel>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.ReceiverId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.NotificationType, opt => opt.MapFrom(src => src.ActivityType.ToString()))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.Status == NotificationStatus.Read))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Created))
                .ReverseMap();
        }
    }
}

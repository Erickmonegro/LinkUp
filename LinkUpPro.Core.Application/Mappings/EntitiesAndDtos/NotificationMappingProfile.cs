using AutoMapper;
using LinkUpPro.Core.Application.DTOs.NotificationDTO;
using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Mappings.EntitiesAndDtos
{
    public class NotificationMappingProfile : Profile
    {
        public NotificationMappingProfile()
        {
            CreateMap<Notification, NotificationDTO>()
                .ForMember(dest => dest.TriggerUserName, opt => opt.Ignore())
                .ForMember(dest => dest.TriggerUserPhotoUrl, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}

using AutoMapper;
using LinkUpPro.Core.Application.DTOs.FriendRequestDTO;
using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Mappings.EntitiesAndDtos
{
    public class FriendRequestMappingProfile : Profile
    {
        public FriendRequestMappingProfile()
        {
            CreateMap<FriendRequest, FriendRequestDTO>()
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Updated, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
                .ForMember(dest => dest.SenderName, opt => opt.Ignore())
                .ForMember(dest => dest.SenderPhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ReceiverName, opt => opt.Ignore())
                .ForMember(dest => dest.ReceiverPhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.MutualFriendsCount, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Created))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.Updated))
                .ForMember(dest => dest.IsHiddenForSender, opt => opt.Ignore())
                .ForMember(dest => dest.IsHiddenForReceiver, opt => opt.Ignore());
        }
    }
}

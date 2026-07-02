using AutoMapper;
using LinkUpPro.Core.Application.DTOs.FriendshipDTO;
using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Mappings.EntitiesAndDtos
{
    public class FriendshipMappingProfile : Profile
    {
        public FriendshipMappingProfile()
        {
            CreateMap<Friendship, FriendshipDTO>()
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Updated, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
                .ForMember(dest => dest.FriendId, opt => opt.Ignore())
                .ForMember(dest => dest.FriendName, opt => opt.Ignore())
                .ForMember(dest => dest.FriendPhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.MutualFriendsCount, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Created))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.Updated))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
        }
    }
}

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
                .ForMember(dest => dest.FriendId, opt => opt.Ignore())
                .ForMember(dest => dest.FriendName, opt => opt.Ignore())
                .ForMember(dest => dest.FriendPhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.MutualFriendsCount, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
        }
    }
}

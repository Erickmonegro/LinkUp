using AutoMapper;
using LinkUpPro.Core.Application.DTOs.FriendshipDTO;
using LinkUpPro.Core.Application.ViewModels.Friendship;

namespace LinkUpPro.Core.Application.Mappings.DtosAndViewModels
{
    public class FriendshipDtoMappingProfile : Profile
    {
        public FriendshipDtoMappingProfile()
        {
            CreateMap<FriendshipDTO, FriendViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.FriendId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FriendName))
                .ForMember(dest => dest.ProfilePhoto, opt => opt.MapFrom(src => src.FriendPhotoUrl))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.FriendId))
                .ForMember(dest => dest.MutualFriendsCount, opt => opt.MapFrom(src => src.MutualFriendsCount))
                .ReverseMap();
        }
    }
}

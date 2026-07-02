using AutoMapper;
using LinkUpPro.Core.Application.DTOs.FriendRequestDTO;
using LinkUpPro.Core.Application.ViewModels.FriendRequest;

namespace LinkUpPro.Core.Application.Mappings.DtosAndViewModels
{
    public class FriendRequestDtoMappingProfile : Profile
    {
        public FriendRequestDtoMappingProfile()
        {
            CreateMap<FriendRequestDTO, FriendRequestViewModel>()
                .ForMember(dest => dest.SenderProfilePhoto, opt => opt.MapFrom(src => src.SenderPhotoUrl))
                .ForMember(dest => dest.ReceiverProfilePhoto, opt => opt.MapFrom(src => src.ReceiverPhotoUrl))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Created))
                .ReverseMap();
        }
    }
}

using AutoMapper;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.ViewModels.User;

namespace LinkUpPro.Core.Application.Mappings.DtosAndViewModels
{
    public class UserDtoMappingProfile : Profile
    {
        public UserDtoMappingProfile()
        {
            CreateMap<UserDTO, UserViewModel>().ReverseMap();
            
            CreateMap<UserDTO, DeleteUserViewModel>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ReverseMap()
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.PhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore());

            CreateMap<UserDTO, UpdateUserViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.CoverImageFile, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.PhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.CoverPhotoUrl, opt => opt.Ignore());

            CreateMap<SaveUserDto, UpdateUserViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.CoverImageFile, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.PhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.CoverPhotoUrl, opt => opt.Ignore());

            CreateMap<SaveUserDto, RegisterUserViewModel>()
                .ForMember(dest => dest.ProfileImageFile, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.PhotoUrl, opt => opt.Ignore());

            CreateMap<LoginDto, LoginViewModel>().ReverseMap();
            CreateMap<ForgotPasswordRequestDto, ForgotPasswordRequestViewModel>().ReverseMap();
            CreateMap<ResetPasswordRequestDto, ResetPasswordRequestViewModel>().ReverseMap();
        }
    }
}

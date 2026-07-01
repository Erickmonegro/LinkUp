using AutoMapper;
using LinkUpPro.Core.Application.DTOs.PostDTO;
using LinkUpPro.Core.Application.ViewModels.Post;

namespace LinkUpPro.Core.Application.Mappings.DtosAndViewModels
{
    public class PostDtoMappingProfile : Profile
    {
        public PostDtoMappingProfile()
        {
            CreateMap<PostDTO, PostViewModel>().ReverseMap();
            CreateMap<PostDTO, SavePostViewModel>().ReverseMap();
        }
    }
}

using AutoMapper;
using LinkUpPro.Core.Application.DTOs.CommentDTO;
using LinkUpPro.Core.Application.ViewModels.Comment;

namespace LinkUpPro.Core.Application.Mappings.DtosAndViewModels
{
    public class CommentDtoMappingProfile : Profile
    {
        public CommentDtoMappingProfile()
        {
            CreateMap<CommentDTO, CommentViewModel>().ReverseMap();
            CreateMap<CommentDTO, SaveCommentViewModel>().ReverseMap();
        }
    }
}

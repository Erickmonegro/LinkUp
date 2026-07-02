using AutoMapper;
using LinkUpPro.Core.Application.DTOs.PostDTO;
using LinkUpPro.Core.Domain.Entities;
using System.Linq;

namespace LinkUpPro.Core.Application.Mappings.EntitiesAndDtos
{
    public class PostMappingProfile : Profile
    {
        public PostMappingProfile()
        {
            CreateMap<Post, PostDTO>()
                .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorPhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Count : 0))
                .ForMember(dest => dest.ReactionsCount, opt => opt.MapFrom(src => src.Reactions != null ? src.Reactions.Count : 0))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Where(c => c.ParentCommentId == null || c.ParentCommentId == 0).ToList() : null))
                .ForMember(dest => dest.Reactions, opt => opt.MapFrom(src => src.Reactions))
                .ReverseMap()
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Reactions, opt => opt.Ignore());
        }
    }
}

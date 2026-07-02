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
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Updated, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
                .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorPhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Count : 0))
                .ForMember(dest => dest.ReactionsCount, opt => opt.MapFrom(src => src.Reactions != null ? src.Reactions.Count : 0))
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.Reactions != null ? src.Reactions.Count(r => r.ReactionType == LinkUpPro.Core.Domain.Common.Enums.ReactionType.Like) : 0))
                .ForMember(dest => dest.DislikesCount, opt => opt.MapFrom(src => src.Reactions != null ? src.Reactions.Count(r => r.ReactionType == LinkUpPro.Core.Domain.Common.Enums.ReactionType.Dislike) : 0))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Where(c => c.ParentCommentId == null || c.ParentCommentId == 0).ToList() : null))
                .ForMember(dest => dest.Reactions, opt => opt.MapFrom(src => src.Reactions))
                .ReverseMap()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Created))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.Updated))
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Reactions, opt => opt.Ignore());
        }
    }
}

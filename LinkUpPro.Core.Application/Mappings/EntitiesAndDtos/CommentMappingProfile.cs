using AutoMapper;
using LinkUpPro.Core.Application.DTOs.CommentDTO;
using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Mappings.EntitiesAndDtos
{
    public class CommentMappingProfile : Profile
    {
        public CommentMappingProfile()
        {
            CreateMap<Comment, CommentDTO>()
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Updated, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
                .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorPhotoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.IsEdited, opt => opt.MapFrom(src => src.UpdatedAt.HasValue && src.UpdatedAt.Value != DateTime.MinValue))
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies))
                .ReverseMap()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Created))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.Updated))
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore());
        }
    }
}

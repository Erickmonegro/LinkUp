using AutoMapper;
using LinkUpPro.Core.Application.DTOs.ReactionDTO;
using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Mappings.EntitiesAndDtos
{
    public class ReactionMappingProfile : Profile
    {
        public ReactionMappingProfile()
        {
            CreateMap<Reaction, ReactionDTO>()
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Updated, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Created))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.Updated))
                .ForMember(dest => dest.Post, opt => opt.Ignore());
        }
    }
}

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
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Post, opt => opt.Ignore());
        }
    }
}

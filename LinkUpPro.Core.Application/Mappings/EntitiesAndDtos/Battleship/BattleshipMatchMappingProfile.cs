using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Mappings.EntitiesAndDTOs.Battleship
{
    public class BattleshipMatchMappingProfile : Profile
    {
        public BattleshipMatchMappingProfile()
        {
            CreateMap<BattleshipMatch, BattleshipMatchDTO>()
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CreatedAt))
                .ReverseMap()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Created));
        }
    }
}

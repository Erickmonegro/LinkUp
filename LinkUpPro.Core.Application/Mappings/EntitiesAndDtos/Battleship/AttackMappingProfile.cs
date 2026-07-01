using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Mappings.EntitiesAndDTOs.Battleship
{
    public class AttackMappingProfile : Profile
    {
        public AttackMappingProfile()
        {
            CreateMap<Attack, AttackDTO>().ReverseMap();
        }
    }
}
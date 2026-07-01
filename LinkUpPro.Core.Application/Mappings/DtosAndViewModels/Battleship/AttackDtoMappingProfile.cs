using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.ViewModels.Battleship;

namespace LinkUpPro.Core.Application.Mappings.DtosAndViewModels.Battleship
{
    public class AttackDtoMappingProfile : Profile
    {
        public AttackDtoMappingProfile()
        {
            CreateMap<AttackDTO, AttackViewModel>().ReverseMap();
        }
    }
}
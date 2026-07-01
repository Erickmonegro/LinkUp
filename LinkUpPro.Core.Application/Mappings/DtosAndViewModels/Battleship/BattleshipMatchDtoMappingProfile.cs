using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.ViewModels.Battleship;

namespace LinkUpPro.Core.Application.Mappings.DtosAndViewModels.Battleship
{
    public class BattleshipMatchDtoMappingProfile : Profile
    {
        public BattleshipMatchDtoMappingProfile()
        {
            CreateMap<BattleshipMatchDTO, BattleshipMatchViewModel>().ReverseMap();

            CreateMap<CreateMatchViewModel, BattleshipMatchDTO>();

            CreateMap<SurrenderViewModel, BattleshipMatchDTO>();
        }
    }
}
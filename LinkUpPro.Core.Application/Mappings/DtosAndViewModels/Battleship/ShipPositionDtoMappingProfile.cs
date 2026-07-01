using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.ViewModels.Battleship;

namespace LinkUpPro.Core.Application.Mappings.DtosAndViewModels.Battleship
{
    public class ShipPositionDtoMappingProfile : Profile
    {
        public ShipPositionDtoMappingProfile()
        {
            CreateMap<ShipPositionDTO, ShipPositionViewModel>().ReverseMap();
        }
    }
}
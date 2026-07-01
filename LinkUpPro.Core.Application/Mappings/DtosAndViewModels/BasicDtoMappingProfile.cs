using AutoMapper;
using LinkUpPro.Core.Application.DTOs;
using LinkUpPro.Core.Application.ViewModels;

namespace LinkUpPro.Core.Application.Mappings.DtosAndViewModels
{
    public class BasicDtoMappingProfile : Profile
    {
        public BasicDtoMappingProfile()
        {

            CreateMap(typeof(BasicDTO<>), typeof(BasicViewModel<>)).ReverseMap();
        }
    }
}

using AutoMapper;
using LinkUpPro.Core.Application.DTOs.ReactionDTO;
using LinkUpPro.Core.Application.ViewModels.Reaction;

namespace LinkUpPro.Core.Application.Mappings.DtosAndViewModels
{
    public class ReactionDtoMappingProfile : Profile
    {
        public ReactionDtoMappingProfile()
        {
            CreateMap<ReactionDTO, ReactionViewModel>().ReverseMap();
            CreateMap<ReactionDTO, SaveReactionViewModel>().ReverseMap();
        }
    }
}

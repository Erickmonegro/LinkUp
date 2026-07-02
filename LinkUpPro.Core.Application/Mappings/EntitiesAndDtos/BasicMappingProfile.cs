using AutoMapper;
using LinkUpPro.Core.Application.DTOs;
using LinkUpPro.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Mappings.EntitiesAndDtos
{
    public class BasicMappingProfile : Profile
    {
        public BasicMappingProfile() { 
        
            CreateMap(typeof(BasicDTO<>), typeof(BasicEntity<>))
                .ForMember("CreatedAt", opt => opt.MapFrom("Created"))
                .ForMember("UpdatedAt", opt => opt.MapFrom("Updated"))
                .ReverseMap()
                .ForMember("Created", opt => opt.MapFrom("CreatedAt"))
                .ForMember("Updated", opt => opt.MapFrom("UpdatedAt"));
        }
    }
}

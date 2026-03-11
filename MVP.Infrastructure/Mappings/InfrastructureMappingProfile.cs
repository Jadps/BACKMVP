using AutoMapper;
using MVP.Domain.Entities;
using MVP.Infrastructure.Identity;
using System.Linq;

namespace MVP.Infrastructure.Mappings;

public class InfrastructureMappingProfile : Profile
{
    public InfrastructureMappingProfile()
    {
        CreateMap<ApplicationUser, Usuario>()
            .ForMember(dest => dest.RoleIds, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.RoleId).ToList()))
            .ReverseMap()
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

        CreateMap<ApplicationRole, Rol>()
            .ReverseMap();
    }
}

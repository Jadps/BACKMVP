using AutoMapper;
using SGEDI.Domain.Entities;
using SGEDI.Application.DTOs;
using SGEDI.Domain.Cifrado;
using SGEDI.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom<CifradoIdResolver, int>(src => src.Id))
            .ForMember(dest => dest.Roles, opt => opt.Ignore());
            
        CreateMap<UsuarioDTO, Usuario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId));

        CreateMap<Rol, RolDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom<CifradoIdResolver, int>(src => src.Id));

        CreateMap<RolDTO, Rol>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<Modulo, ModuloDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom<CifradoIdResolver, int>(src => src.Id))
            .ForMember(dest => dest.PadreId, opt => opt.MapFrom<CifradoNullableIdResolver, int?>(src => src.PadreId))
            .ForMember(dest => dest.SubModulos, opt => opt.MapFrom(src => src.SubModulos));

        CreateMap<Tenant, TenantDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom<CifradoIdResolver, int>(src => src.Id));
            
        CreateMap<TenantDTO, Tenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
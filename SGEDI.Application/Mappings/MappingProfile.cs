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
            .ForMember(dest => dest.NombreRol, opt => opt.MapFrom(src => src.Rol != null ? src.Rol.Name : "Sin Rol"));
            
        CreateMap<UsuarioDTO, Usuario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.RolId, opt => opt.MapFrom(src => src.RolId));
        CreateMap<Rol, RolDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom<CifradoIdResolver, int>(src => src.Id));

        CreateMap<Modulo, ModuloDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom<CifradoIdResolver, int>(src => src.Id))
            .ForMember(dest => dest.PadreId, opt => opt.MapFrom<CifradoNullableIdResolver, int?>(src => src.PadreId))
            .ForMember(dest => dest.SubModulos, opt => opt.MapFrom(src => src.SubModulos));
    }
}
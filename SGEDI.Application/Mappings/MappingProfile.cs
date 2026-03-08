using AutoMapper;
using SGEDI.Domain.Entities;
using SGEDI.Application.DTOs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Uid))
            .ForMember(dest => dest.Roles, opt => opt.Ignore());
            
        CreateMap<UsuarioDTO, Usuario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId));

        CreateMap<Rol, RolDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Uid));

        CreateMap<RolDTO, Rol>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => src.Id));

        CreateMap<Modulo, ModuloDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Uid))
            .ForMember(dest => dest.PadreId, opt => opt.MapFrom(src => src.Padre != null ? src.Padre.Uid : (Guid?)null))
            .ForMember(dest => dest.SubModulos, opt => opt.MapFrom(src => src.SubModulos));

        CreateMap<Tenant, TenantDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Uid));
            
        CreateMap<TenantDTO, Tenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => src.Id));
    }
}
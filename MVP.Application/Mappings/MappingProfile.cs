using AutoMapper;
using MVP.Domain.Entities;
using MVP.Application.DTOs;

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

        CreateMap<OnboardingRequestDTO, Tenant>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.EmpresaNombre))
            .ForMember(dest => dest.Dominio, opt => opt.MapFrom(src => src.Dominio))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<OnboardingRequestDTO, Usuario>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.AdminEmail))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.AdminEmail))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.AdminNombre))
            .ForMember(dest => dest.PrimerApellido, opt => opt.MapFrom(src => src.AdminPrimerApellido))
            .ForMember(dest => dest.SegundoApellido, opt => opt.MapFrom(src => src.AdminSegundoApellido))
            .ForMember(dest => dest.FriendlyName, opt => opt.MapFrom(src => src.AdminNombre));
    }
}
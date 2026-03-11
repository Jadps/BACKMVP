using AutoMapper;
using MVP.Application.DTOs;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        CreateMap<Usuario, UsuarioDTO>()
            .ConstructUsing((src, _) => new UsuarioDTO(
                src.Uid,
                src.Email ?? string.Empty,
                src.Nombre ?? string.Empty,
                src.PrimerApellido ?? string.Empty,
                src.SegundoApellido,
                null,                
                src.TenantId,
                new List<RolDTO>(),  
                src.NombreCompleto
            ))
            .ForAllMembers(opt => opt.Ignore());

        CreateMap<UsuarioDTO, Usuario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId));

        CreateMap<Rol, RolDTO>()
            .ConstructUsing((src, _) => new RolDTO(
                src.Uid,
                src.Name ?? string.Empty,
                src.Descripcion,
                src.TenantId
            ))
            .ForAllMembers(opt => opt.Ignore());

        CreateMap<RolDTO, Rol>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => src.Id));

        CreateMap<Modulo, ModuloDTO>()
            .ConstructUsing((src, ctx) => new ModuloDTO(
                src.Uid,
                src.Descripcion,
                src.Icono,
                src.Accion,          
                src.Orden ?? 0,
                src.Padre != null ? src.Padre.Uid : (Guid?)null,
                ctx.Mapper.Map<List<ModuloDTO>>(src.SubModulos)
            ))
            .ForAllMembers(opt => opt.Ignore());

        CreateMap<Tenant, TenantDTO>()
            .ConstructUsing((src, _) => new TenantDTO(
                src.Uid,
                src.Nombre ?? string.Empty,
                src.Dominio,
                src.Borrado,
                src.FechaCreacion
            ))
            .ForAllMembers(opt => opt.Ignore());

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
using AutoMapper;
using MVP.Application.DTOs;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;

namespace MVP.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Usuario, UsuarioDTO>()
                .ForMember(dest => dest.Id,       opt => opt.MapFrom(src => src.Uid))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.Tenant != null ? src.Tenant.Uid : (Guid?)null))
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.Roles,    opt => opt.Ignore());

            CreateMap<UsuarioDTO, Usuario>()
                .ForMember(dest => dest.Id,       opt => opt.Ignore())
                .ForMember(dest => dest.Uid,      opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore());

            CreateMap<Rol, RolDTO>()
                .ForMember(dest => dest.Id,       opt => opt.MapFrom(src => src.Uid))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.Tenant != null ? src.Tenant.Uid : (Guid?)null));

            CreateMap<RolDTO, Rol>()
                .ForMember(dest => dest.Id,       opt => opt.Ignore())
                .ForMember(dest => dest.Uid,      opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore());

            CreateMap<Modulo, ModuloDTO>()
                .ForMember(dest => dest.Id,      opt => opt.MapFrom(src => src.Uid))
                .ForMember(dest => dest.PadreId, opt => opt.MapFrom(src => src.Padre != null ? src.Padre.Uid : (Guid?)null));

            CreateMap<ModuloDTO, Modulo>()
                .ForMember(dest => dest.Id,      opt => opt.Ignore())
                .ForMember(dest => dest.Uid,     opt => opt.Ignore())
                .ForMember(dest => dest.PadreId, opt => opt.Ignore());

            CreateMap<Tenant, TenantDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Uid));

            CreateMap<TenantDTO, Tenant>()
                .ForMember(dest => dest.Id,  opt => opt.Ignore())
                .ForMember(dest => dest.Uid, opt => opt.Ignore());

            CreateMap<OnboardingRequestDTO, Tenant>()
                .ForMember(dest => dest.Nombre,        opt => opt.MapFrom(src => src.EmpresaNombre))
                .ForMember(dest => dest.Dominio,       opt => opt.MapFrom(src => src.Dominio))
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<OnboardingRequestDTO, Usuario>()
                .ForMember(dest => dest.UserName,        opt => opt.MapFrom(src => src.AdminEmail))
                .ForMember(dest => dest.Email,           opt => opt.MapFrom(src => src.AdminEmail))
                .ForMember(dest => dest.Nombre,          opt => opt.MapFrom(src => src.AdminNombre))
                .ForMember(dest => dest.PrimerApellido,  opt => opt.MapFrom(src => src.AdminPrimerApellido))
                .ForMember(dest => dest.SegundoApellido, opt => opt.MapFrom(src => src.AdminSegundoApellido))
                .ForMember(dest => dest.FriendlyName,    opt => opt.MapFrom(src => src.AdminNombre));
        }
    }
}
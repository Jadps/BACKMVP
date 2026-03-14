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
                .ForCtorParam("Id",       opt => opt.MapFrom(src => src.Uid))
                .ForCtorParam("Password", opt => opt.MapFrom(src => (string?)null))
                .ForCtorParam("Roles",    opt => opt.MapFrom(src => new List<RolDTO>()));

            CreateMap<UsuarioDTO, Usuario>()
                .ForMember(dest => dest.Id,  opt => opt.Ignore())
                .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => src.Id));

            CreateMap<Rol, RolDTO>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Uid));

            CreateMap<RolDTO, Rol>()
                .ForMember(dest => dest.Id,  opt => opt.Ignore())
                .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => src.Id));

            CreateMap<Modulo, ModuloDTO>();

            CreateMap<Tenant, TenantDTO>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Uid));

            CreateMap<TenantDTO, Tenant>()
                .ForMember(dest => dest.Id,  opt => opt.Ignore())
                .ForMember(dest => dest.Uid, opt => opt.MapFrom(src => src.Id));

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
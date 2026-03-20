using AutoMapper;
using MVP.Application.DTOs;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MVP.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id,       opt => opt.MapFrom(src => src.Uid))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.Tenant != null ? src.Tenant.Uid : (Guid?)null))
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.Roles,    opt => opt.Ignore());

            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Id,       opt => opt.Ignore())
                .ForMember(dest => dest.Uid,      opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore());

            CreateMap<User, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Uid, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());

            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Id,       opt => opt.MapFrom(src => src.Uid))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.Tenant != null ? src.Tenant.Uid : (Guid?)null))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.RoleModules));

            CreateMap<RoleModule, RolePermissionDto>()
                .ForMember(dest => dest.ModuleId,  opt => opt.MapFrom(src => src.Module.Uid))
                .ForMember(dest => dest.Permission, opt => opt.MapFrom(src => src.Permission));

            CreateMap<RoleDto, Role>()
                .ForMember(dest => dest.Id,       opt => opt.Ignore())
                .ForMember(dest => dest.Uid,      opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore());

            CreateMap<Module, ModuleDto>()
                .ForMember(dest => dest.Id,       opt => opt.MapFrom(src => src.Uid))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Uid : (Guid?)null));

            CreateMap<ModuleDto, Module>()
                .ForMember(dest => dest.Id,       opt => opt.Ignore())
                .ForMember(dest => dest.Uid,      opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore());

            CreateMap<Tenant, TenantDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Uid));

            CreateMap<TenantDto, Tenant>()
                .ForMember(dest => dest.Id,  opt => opt.Ignore())
                .ForMember(dest => dest.Uid, opt => opt.Ignore());

            CreateMap<ModuleDto, ModuleDto>();

        }
    }
}

using AutoMapper;
using Microsoft.Extensions.Caching.Hybrid;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Catalogs;
using MVP.Application.Interfaces.Repositories;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Application.Services.Catalogs;

public class CatalogService(
    IIdentityService identityService,
    ICurrentTenantService currentTenantService,
    IMapper mapper, 
    ICatalogRepository catalogRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache) : ICatalogService
{
    private const string RolesCacheKey = "global_roles";
    private const string ModulesCacheKey = "modules_cache";

    public async Task<ApplicationResult<List<RoleDto>>> GetRolesAsync()
    {
        var roles = await cache.GetOrCreateAsync(
            RolesCacheKey, 
            async cancelToken =>
            {
                var roles = await identityService.GetActiveRolesAsync();
                return mapper.Map<List<RoleDto>>(roles);
            },
            tags: ["global_roles"]
        );
        
        return ApplicationResult<List<RoleDto>>.Success(roles ?? []);
    }

    public async Task<ApplicationResult> CreateRoleAsync(RoleDto dto)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description,
                IsDeleted = false
            };

            if (dto.Permissions != null && dto.Permissions.Any())
            {
                var moduleUids = dto.Permissions.Select(p => p.ModuleId).Distinct();
                var modules = await catalogRepository.GetModulesByUidsAsync(moduleUids, default);

                foreach (var p in dto.Permissions)
                {
                    var module = modules.FirstOrDefault(m => m.Uid == p.ModuleId);
                    if (module != null)
                    {
                        role.RoleModules.Add(new RoleModule
                        {
                            ModuleId = module.Id,
                            Permission = p.Permission
                        });
                    }
                }
            }

            var result = await identityService.CreateRoleAsync(role);
            if (!result.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync();
                return result;
            }

            await unitOfWork.CommitTransactionAsync();
            await cache.RemoveByTagAsync("global_roles");
            await cache.RemoveByTagAsync("all_menus");
            return ApplicationResult.Success();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return ApplicationResult.Failure($"Error al crear el rol: {ex.Message}");
        }
    }

    public async Task<ApplicationResult> UpdateRoleAsync(RoleDto dto)
    {
        if (dto.Id == null) return ApplicationResult.Failure("El ID del rol es requerido para actualizar.");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            var role = await catalogRepository.GetRoleWithPermissionsByUidAsync(dto.Id.Value, default);

            if (role == null)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ApplicationResult.Failure("Rol no encontrado.", ErrorType.NotFound);
            }

            role.Name = dto.Name;
            role.Description = dto.Description;

            if (role.RoleModules != null && role.RoleModules.Any())
            {
                catalogRepository.DeleteRolePermissions(role.RoleModules);
            }

            if (dto.Permissions != null && dto.Permissions.Any())
            {
                var moduleUids = dto.Permissions.Select(p => p.ModuleId).Distinct();
                var modules = await catalogRepository.GetModulesByUidsAsync(moduleUids, default);

                foreach (var p in dto.Permissions)
                {
                    var module = modules.FirstOrDefault(m => m.Uid == p.ModuleId);
                    if (module != null)
                    {
                        role.RoleModules?.Add(new RoleModule
                        {
                            RoleId = role.Id,
                            ModuleId = module.Id,
                            Permission = p.Permission
                        });
                    }
                }
            }

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitTransactionAsync();
            await cache.RemoveByTagAsync("global_roles");
            await cache.RemoveByTagAsync("all_menus");
            return ApplicationResult.Success();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return ApplicationResult.Failure($"Error al actualizar el rol: {ex.Message}");
        }
    }

    public async Task<ApplicationResult<List<ModuleDto>>> GetMenuModulesAsync()
    {
        var userIdJson = currentTenantService.UserId;
        if (string.IsNullOrEmpty(userIdJson) || !Guid.TryParse(userIdJson, out var userUid))
        {
            return ApplicationResult<List<ModuleDto>>.Success([]);
        }

        var tenantId = currentTenantService.TenantId;
        var cacheKey = $"menu_user_{userUid}";
        var tags = new List<string> { $"user_{userUid}_menu", "all_menus" };
        if (tenantId.HasValue)
            tags.Add($"tenant_{tenantId.Value}_menus");
        else
            tags.Add("superadmin_menus");

        var filteredTopLevel = await cache.GetOrCreateAsync(
            cacheKey,
            async cancelToken =>
            {
                var modules = await cache.GetOrCreateAsync(
                    ModulesCacheKey,
                    async ct => {
                        var entities = await catalogRepository.GetActiveModulesWithSubModulesAsync(ct);
                        return mapper.Map<List<ModuleDto>>(entities);
                    },
                    tags: ["modules_cache"]
                );

                if (currentTenantService.IsSuperAdmin)
                {
                    return modules
                        .Where(m => m.ParentId == null)
                        .Select(m => mapper.Map<ModuleDto>(m))
                        .OrderBy(m => m.Order)
                        .ToList();
                }

                var allowedModuleUids = await catalogRepository.GetAllowedModuleIdsForUserAsync(userUid, cancelToken);

                return modules
                    .Where(m => m.ParentId == null && allowedModuleUids.Contains(m.Id ?? Guid.Empty))
                    .Select(m => {
                        var dto = mapper.Map<ModuleDto>(m);
                        dto.SubModules = m.SubModules
                            .Where(sm => allowedModuleUids.Contains(sm.Id ?? Guid.Empty))
                            .OrderBy(s => s.Order)
                            .ToList();
                        return dto;
                    })
                    .OrderBy(m => m.Order)
                    .ToList();
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(1) },
            tags
        );

        return ApplicationResult<List<ModuleDto>>.Success(filteredTopLevel ?? []);
    }
}

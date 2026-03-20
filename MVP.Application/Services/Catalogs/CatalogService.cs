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
        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            IsDeleted = false
        };

        var result = await identityService.CreateRoleAsync(role);
        if (!result.IsSuccess) return result;
        
        await cache.RemoveByTagAsync("global_roles");
        return ApplicationResult.Success();
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
        var tags = new List<string> { $"user_{userUid}_menu" };
        if (tenantId.HasValue)
            tags.Add($"tenant_{tenantId.Value}_menus");
        else
            tags.Add("superadmin_menus");

        var filteredTopLevel = await cache.GetOrCreateAsync(
            cacheKey,
            async cancelToken =>
            {
                var modules = await catalogRepository.GetActiveModulesWithSubModulesAsync(cancelToken);

                if (currentTenantService.IsSuperAdmin)
                {
                    return modules
                        .Where(m => m.ParentId == null)
                        .Select(m => {
                            var dto = mapper.Map<ModuleDto>(m);
                            dto.SubModules = m.SubModules
                                .Select(sm => mapper.Map<ModuleDto>(sm))
                                .OrderBy(s => s.Order) 
                                .ToList();
                            return dto;
                        })
                        .OrderBy(m => m.Order)
                        .ToList();
                }

                var allowedModuleIds = await catalogRepository.GetAllowedModuleIdsForUserAsync(userUid, cancelToken);

                return modules
                    .Where(m => m.ParentId == null && allowedModuleIds.Contains(m.Id))
                    .Select(m => {
                        var dto = mapper.Map<ModuleDto>(m);
                        dto.SubModules = m.SubModules
                            .Where(sm => allowedModuleIds.Contains(sm.Id))
                            .Select(sm => mapper.Map<ModuleDto>(sm))
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

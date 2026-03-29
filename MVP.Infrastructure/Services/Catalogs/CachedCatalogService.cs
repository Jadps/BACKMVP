using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MVP.Application.Constants;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Catalogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Services.Catalogs;

public class CachedCatalogService(
    ICatalogService innerService,
    HybridCache cache,
    ICurrentTenantService currentTenantService,
    ILogger<CachedCatalogService> logger) : ICatalogService
{
    private string RolesCacheKey => currentTenantService.IsSuperAdmin
        ? "cache_key::roles_list::superadmin"
        : $"cache_key::roles_list::tenant_{currentTenantService.TenantId}";

    public async Task<ApplicationResult<List<RoleDto>>> GetRolesAsync()
    {
        var key = RolesCacheKey;
        logger.LogDebug("[Cache] GetRolesAsync — key={Key}", key);

        var cachedData = await cache.GetOrCreateAsync(
            key,
            async cancelToken =>
            {
                logger.LogDebug("[Cache] MISS — fetching roles from DB (key={Key})", key);
                var response = await innerService.GetRolesAsync();
                if (!response.IsSuccess)
                    throw new InvalidOperationException(response.ErrorMessage ?? "Error fetching roles");
                return response.Data ?? [];
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(30) },
            tags: [CacheTags.GlobalRoles],
            cancellationToken: CancellationToken.None
        );

        return ApplicationResult<List<RoleDto>>.Success(cachedData ?? []);
    }

    public async Task<ApplicationResult> CreateRoleAsync(RoleDto dto)
    {
        var result = await innerService.CreateRoleAsync(dto);

        if (result.IsSuccess)
        {
            await InvalidateRolesAndMenusAsync();
        }

        return result;
    }

    public async Task<ApplicationResult> UpdateRoleAsync(RoleDto dto)
    {
        var result = await innerService.UpdateRoleAsync(dto);

        if (result.IsSuccess)
        {
            await InvalidateRolesAndMenusAsync();
        }

        return result;
    }

    public async Task<ApplicationResult<List<ModuleDto>>> GetMenuModulesAsync()
    {
        var userIdStr = currentTenantService.UserId;
        var tenantId = currentTenantService.TenantId;

        if (string.IsNullOrEmpty(userIdStr))
        {
            logger.LogDebug("[Cache] GetMenuModulesAsync — no userId, bypassing cache");
            return await innerService.GetMenuModulesAsync();
        }

        var cacheKey = CacheKeys.UserMenu(userIdStr);
        logger.LogDebug("[Cache] GetMenuModulesAsync — key={Key}", cacheKey);

        var tags = new List<string> { CacheTags.UserMenu(userIdStr), CacheTags.AllMenus };
        if (tenantId.HasValue)
            tags.Add(CacheTags.TenantMenus(tenantId.Value));
        else
            tags.Add(CacheTags.SuperAdminMenus);

        var cachedData = await cache.GetOrCreateAsync(
            cacheKey,
            async cancelToken =>
            {
                logger.LogDebug("[Cache] MISS — fetching menu from DB (key={Key})", cacheKey);
                var response = await innerService.GetMenuModulesAsync();
                if (!response.IsSuccess)
                    throw new InvalidOperationException(response.ErrorMessage ?? "Error fetching menu modules");
                return response.Data ?? [];
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(30) },
            tags,
            cancellationToken: CancellationToken.None
        );

        return ApplicationResult<List<ModuleDto>>.Success(cachedData ?? []);
    }

    private async Task InvalidateRolesAndMenusAsync()
    {
        logger.LogInformation("[Cache] Invalidating roles + menus after role change");

        await cache.RemoveAsync("cache_key::roles_list::superadmin");
        await cache.RemoveAsync($"cache_key::roles_list::tenant_{currentTenantService.TenantId}");

        await cache.RemoveByTagAsync(CacheTags.GlobalRoles, CancellationToken.None);
        await cache.RemoveByTagAsync(CacheTags.AllMenus, CancellationToken.None);
        await cache.RemoveByTagAsync(CacheTags.ModulesCache, CancellationToken.None);

        logger.LogInformation("[Cache] Invalidation complete");
    }
}

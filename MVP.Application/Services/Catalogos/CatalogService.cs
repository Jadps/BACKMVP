using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Catalogos;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Application.Services.Catalogos;

public class CatalogService(
    IIdentityService identityService,
    IMapper mapper, 
    IApplicationDbContext context,
    IMemoryCache cache) : ICatalogService
{
    private const string RolesCacheKey = "roles_cache";
    private const string ModulesCacheKey = "modules_cache";

    public async Task<ApplicationResult<List<RoleDto>>> GetRolesAsync()
    {
        var roles = await cache.GetOrCreateAsync(RolesCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var roles = await identityService.GetActiveRolesAsync();
            return mapper.Map<List<RoleDto>>(roles);
        }) ?? [];
        
        return ApplicationResult<List<RoleDto>>.Success(roles);
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
        
        cache.Remove(RolesCacheKey);
        return ApplicationResult.Success();
    }

    public async Task<ApplicationResult<List<ModuleDto>>> GetMenuModulesAsync()
    {
        var modules = await cache.GetOrCreateAsync(ModulesCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return await context.Modules
                .AsNoTracking()
                .Where(m => m.ParentId == null)
                .OrderBy(m => m.Order)
                .ProjectTo<ModuleDto>(mapper.ConfigurationProvider)
                .ToListAsync();
        }) ?? [];

        return ApplicationResult<List<ModuleDto>>.Success(modules);
    }
}

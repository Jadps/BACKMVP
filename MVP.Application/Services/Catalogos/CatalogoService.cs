using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Catalogos;
using MVP.Domain.Entities;
using MVP.Domain.Interfaces;

namespace MVP.Application.Services.Catalogos;

public class CatalogoService(
    IIdentityService identityService,
    IMapper mapper, 
    IApplicationDbContext context,
    IMemoryCache cache) : ICatalogoService
{
    private const string RolesCacheKey = "roles_cache";
    private const string ModulosCacheKey = "modulos_cache";

    public async Task<List<RolDTO>> GetRolesAsync()
    {
        return await cache.GetOrCreateAsync(RolesCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var roles = await identityService.GetRolesActivosAsync();
            return mapper.Map<List<RolDTO>>(roles);
        }) ?? [];
    }

    public async Task<ApplicationResult> CrearRolAsync(RolDTO dto)
    {
        var rol = new Rol
        {
            Name = dto.Name,
            Descripcion = dto.Descripcion,
            Borrado = false
        };

        var result = await identityService.CrearRolAsync(rol);
        if (!result.IsSuccess) return result;
        
        cache.Remove(RolesCacheKey);
        return ApplicationResult.Success();
    }

    public async Task<List<ModuloDTO>> GetModulosMenuAsync()
    {
        return await cache.GetOrCreateAsync(ModulosCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return await context.Modulos
                .AsNoTracking()
                .Where(m => m.PadreId == null)
                .OrderBy(m => m.Orden)
                .ProjectTo<ModuloDTO>(mapper.ConfigurationProvider)
                .ToListAsync();
        }) ?? [];
    }
}

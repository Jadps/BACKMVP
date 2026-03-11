using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Domain.Entities;

namespace MVP.Application.Services;

public class TenantService(IApplicationDbContext context, IMapper mapper) : ITenantService
{
    public async Task<List<TenantDTO>> GetTodosAsync()
    {
        return await context.Tenants.AsNoTracking()
            .ProjectTo<TenantDTO>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<PagedResult<TenantDTO>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = context.Tenants.AsNoTracking();
        var totalCount = await query.CountAsync();
        var items = await query
            .ProjectTo<TenantDTO>(mapper.ConfigurationProvider)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TenantDTO>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<ApplicationResult<TenantDTO>> GetByIdAsync(Guid id)
    {
        var tenant = await context.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Uid == id);
            
        return tenant == null 
            ? ApplicationResult<TenantDTO>.Failure("Inquilino no encontrado.", ErrorType.NotFound)
            : ApplicationResult<TenantDTO>.Success(mapper.Map<TenantDTO>(tenant));
    }

    public async Task<ApplicationResult<Guid>> CrearAsync(TenantDTO dto)
    {
        var tenant = mapper.Map<Tenant>(dto);
        tenant.FechaCreacion = DateTime.UtcNow;
        
        await context.Tenants.AddAsync(tenant);
        await context.SaveChangesAsync();
        
        return ApplicationResult<Guid>.Success(tenant.Uid);
    }

    public async Task<ApplicationResult> ActualizarAsync(TenantDTO dto)
    {
        if (dto.Id == null) 
            return ApplicationResult.Failure("El Id no puede ser nulo", ErrorType.Validation);
        
        var tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Uid == dto.Id.Value);
        
        if (tenant == null)
            return ApplicationResult.Failure("El inquilino no existe.", ErrorType.NotFound);

        mapper.Map(dto, tenant);
        await context.SaveChangesAsync();
        
        return ApplicationResult.Success();
    }

    public async Task<ApplicationResult> EliminarAsync(Guid id)
    {
        var tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Uid == id);
            
        if (tenant == null)
            return ApplicationResult.Failure("El inquilino no existe.", ErrorType.NotFound);

        context.Tenants.Remove(tenant);
        await context.SaveChangesAsync();
        
        return ApplicationResult.Success();
    }
}

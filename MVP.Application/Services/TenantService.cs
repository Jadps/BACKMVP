using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Application.Services;

public class TenantService(IApplicationDbContext context, IMapper mapper) : ITenantService
{
    public async Task<ApplicationResult<List<TenantDto>>> GetAllAsync()
    {
        var tenants = await context.Tenants.AsNoTracking()
            .Where(t => !t.IsDeleted)
            .ProjectTo<TenantDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return ApplicationResult<List<TenantDto>>.Success(tenants);
    }

    public async Task<ApplicationResult<TenantDto>> GetByUidAsync(Guid id)
    {
        var tenant = await context.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Uid == id);
            
        return tenant == null 
            ? ApplicationResult<TenantDto>.Failure("Tenant not found.", ErrorType.NotFound)
            : ApplicationResult<TenantDto>.Success(mapper.Map<TenantDto>(tenant));
    }

    public async Task<ApplicationResult<Guid>> CreateAsync(TenantDto dto)
    {
        var tenant = new Tenant
        {
            Uid = Guid.NewGuid(),
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        await context.Tenants.AddAsync(tenant);
        await context.SaveChangesAsync();
        
        return ApplicationResult<Guid>.Success(tenant.Uid);
    }

    public async Task<ApplicationResult> UpdateAsync(TenantDto dto)
    {
        if (dto.Id == null) return ApplicationResult.Failure("Id cannot be null.", ErrorType.Validation);
        
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Uid == dto.Id.Value);
        if (tenant == null) return ApplicationResult.Failure("Tenant does not exist.", ErrorType.NotFound);

        tenant.Name = dto.Name;
        await context.SaveChangesAsync();
        
        return ApplicationResult.Success();
    }

    public async Task<ApplicationResult> DeleteAsync(Guid id)
    {
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Uid == id);
        if (tenant == null) return ApplicationResult.Failure("Tenant does not exist.", ErrorType.NotFound);

        tenant.IsDeleted = true;
        await context.SaveChangesAsync();
        
        return ApplicationResult.Success();
    }
}

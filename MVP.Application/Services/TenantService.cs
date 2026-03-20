using AutoMapper;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Repositories;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVP.Application.Services;

public class TenantService(ITenantRepository tenantRepository, IMapper mapper) : ITenantService
{
    public async Task<ApplicationResult<List<TenantDto>>> GetAllAsync()
    {
        var tenants = await tenantRepository.GetAllActiveAsync();
        var dtos = mapper.Map<List<TenantDto>>(tenants);
        return ApplicationResult<List<TenantDto>>.Success(dtos);
    }

    public async Task<ApplicationResult<TenantDto>> GetByUidAsync(Guid id)
    {
        var tenant = await tenantRepository.GetByUidAsync(id);
            
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
        
        await tenantRepository.AddAsync(tenant);
        await tenantRepository.SaveChangesAsync();
        
        return ApplicationResult<Guid>.Success(tenant.Uid);
    }

    public async Task<ApplicationResult> UpdateAsync(TenantDto dto)
    {
        if (dto.Id == null) return ApplicationResult.Failure("Id cannot be null.", ErrorType.Validation);
        
        var tenant = await tenantRepository.GetByUidAsync(dto.Id.Value);
        if (tenant == null) return ApplicationResult.Failure("Tenant does not exist.", ErrorType.NotFound);

        tenant.Name = dto.Name;
        await tenantRepository.UpdateAsync(tenant);
        await tenantRepository.SaveChangesAsync();
        
        return ApplicationResult.Success();
    }

    public async Task<ApplicationResult> DeleteAsync(Guid id)
    {
        var tenant = await tenantRepository.GetByUidAsync(id);
        if (tenant == null) return ApplicationResult.Failure("Tenant does not exist.", ErrorType.NotFound);

        tenant.IsDeleted = true;
        await tenantRepository.UpdateAsync(tenant);
        await tenantRepository.SaveChangesAsync();
        
        return ApplicationResult.Success();
    }
}

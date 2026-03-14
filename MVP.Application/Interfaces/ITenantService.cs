using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces;

public interface ITenantService
{
    Task<ApplicationResult<List<TenantDto>>> GetAllAsync();
    Task<ApplicationResult<TenantDto>> GetByUidAsync(Guid id);
    Task<ApplicationResult<Guid>> CreateAsync(TenantDto dto);
    Task<ApplicationResult> UpdateAsync(TenantDto dto);
    Task<ApplicationResult> DeleteAsync(Guid id);
}

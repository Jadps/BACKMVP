using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces;
using SGEDI.Domain.Entities;
using SGEDI.Domain.Interfaces;

namespace SGEDI.Application.Services;

public class TenantService : ITenantService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public TenantService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<List<TenantDTO>> GetTodosAsync()
    {
        var tenants = await _uow.Repository<Tenant>().GetAllAsync();
        return _mapper.Map<List<TenantDTO>>(tenants);
    }

    public async Task<TenantDTO?> GetByIdAsync(Guid id)
    {
        var tenant = await _uow.Repository<Tenant>().GetFirstOrDefaultAsync(t => t.Uid == id);
        return tenant == null ? null : _mapper.Map<TenantDTO>(tenant);
    }

    public async Task<Guid> CrearAsync(TenantDTO dto)
    {
        var tenant = _mapper.Map<Tenant>(dto);
        tenant.FechaCreacion = DateTime.UtcNow;
        
        await _uow.Repository<Tenant>().AddAsync(tenant);
        await _uow.CommitAsync();
        
        return tenant.Uid;
    }

    public async Task ActualizarAsync(TenantDTO dto)
    {
        if (dto.Id == null) throw new ArgumentException("El Id no puede ser nulo");
        var tenant = await _uow.Repository<Tenant>().GetFirstOrDefaultAsync(t => t.Uid == dto.Id.Value);
        
        if (tenant != null)
        {
            _mapper.Map(dto, tenant);
            
            _uow.Repository<Tenant>().Update(tenant);
            await _uow.CommitAsync();
        }
    }
}

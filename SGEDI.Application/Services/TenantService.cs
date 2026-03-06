using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces;
using SGEDI.Domain.Cifrado;
using SGEDI.Domain.Entities;
using SGEDI.Domain.Interfaces;

namespace SGEDI.Application.Services;

public class TenantService : ITenantService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICifradoService _cifrado;

    public TenantService(IUnitOfWork uow, IMapper mapper, ICifradoService cifrado)
    {
        _uow = uow;
        _mapper = mapper;
        _cifrado = cifrado;
    }

    public async Task<List<TenantDTO>> GetTodosAsync()
    {
        var tenants = await _uow.Repository<Tenant>().GetAllAsync();
        return _mapper.Map<List<TenantDTO>>(tenants);
    }

    public async Task<TenantDTO?> GetByIdAsync(string id)
    {
        int decodedId = int.Parse(_cifrado.Desencriptar(id));
        var tenant = await _uow.Repository<Tenant>().GetFirstOrDefaultAsync(t => t.Id == decodedId);
        return tenant == null ? null : _mapper.Map<TenantDTO>(tenant);
    }

    public async Task<int> CrearAsync(TenantDTO dto)
    {
        var tenant = _mapper.Map<Tenant>(dto);
        tenant.FechaCreacion = DateTime.UtcNow;
        
        await _uow.Repository<Tenant>().AddAsync(tenant);
        await _uow.CommitAsync();
        
        return tenant.Id;
    }

    public async Task ActualizarAsync(TenantDTO dto)
    {
        int decodedId = int.Parse(_cifrado.Desencriptar(dto.Id!));
        var tenant = await _uow.Repository<Tenant>().GetFirstOrDefaultAsync(t => t.Id == decodedId);
        
        if (tenant != null)
        {
            _mapper.Map(dto, tenant);
            
            _uow.Repository<Tenant>().Update(tenant);
            await _uow.CommitAsync();
        }
    }
}

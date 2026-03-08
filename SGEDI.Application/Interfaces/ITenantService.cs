using System.Collections.Generic;
using System.Threading.Tasks;
using SGEDI.Application.DTOs;

namespace SGEDI.Application.Interfaces;

public interface ITenantService
{
    Task<List<TenantDTO>> GetTodosAsync();
    Task<TenantDTO?> GetByIdAsync(Guid id);
    Task<Guid> CrearAsync(TenantDTO dto);
    Task ActualizarAsync(TenantDTO dto);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces;

public interface ITenantService
{
    Task<List<TenantDTO>> GetTodosAsync();
    Task<PagedResult<TenantDTO>> GetPagedAsync(int pageNumber, int pageSize);
    Task<TenantDTO?> GetByIdAsync(Guid id);
    Task<Guid> CrearAsync(TenantDTO dto);
    Task ActualizarAsync(TenantDTO dto);
    Task EliminarAsync(Guid id);
}

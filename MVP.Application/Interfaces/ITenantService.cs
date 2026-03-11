using System.Collections.Generic;
using System.Threading.Tasks;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces;

public interface ITenantService
{
    Task<List<TenantDTO>> GetTodosAsync();
    Task<PagedResult<TenantDTO>> GetPagedAsync(int pageNumber, int pageSize);
    Task<ApplicationResult<TenantDTO>> GetByIdAsync(Guid id);
    Task<ApplicationResult<Guid>> CrearAsync(TenantDTO dto);
    Task<ApplicationResult> ActualizarAsync(TenantDTO dto);
    Task<ApplicationResult> EliminarAsync(Guid id);
}

using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces.Usuarios
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDTO>> GetTodosAsync();
        Task<PagedResult<UsuarioDTO>> GetPagedAsync(int pageNumber, int pageSize);
        Task<ApplicationResult<UsuarioDTO>> GetByIdAsync(Guid id);
        Task<ApplicationResult<UsuarioDTO>> GetPerfilActualAsync();
        Task<ApplicationResult> CrearAsync(UsuarioDTO dto);
        Task<ApplicationResult> ActualizarAsync(UsuarioDTO dto);
        Task<ApplicationResult> BorrarAsync(Guid id);
    }
}

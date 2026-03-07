using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGEDI.Application.Interfaces.Usuarios
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDTO>> GetTodosAsync();
        Task<UsuarioDTO?> GetByIdAsync(string idCifrado);
        Task<ApplicationResult> CrearAsync(UsuarioDTO dto);
        Task<ApplicationResult> ActualizarAsync(UsuarioDTO dto);
        Task<bool> BorrarAsync(string idCifrado);
    }
}

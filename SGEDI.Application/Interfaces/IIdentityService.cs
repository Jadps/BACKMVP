using SGEDI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGEDI.Application.Interfaces;

public interface IIdentityService
{
    Task<List<Usuario>> GetUsuariosActivosAsync();
    Task<Usuario?> GetUsuarioActivoByIdAsync(int id);
    Task<ApplicationResult> CrearUsuarioAsync(Usuario usuario, string password, List<string> rolesNombres);
    Task<ApplicationResult> ActualizarUsuarioAsync(Usuario usuario, List<string> rolesNombres);
    Task<bool> BorrarUsuarioAsync(int userId);
    Task<List<Rol>> GetRolesByIdsAsync(IEnumerable<int> roleIds);
}

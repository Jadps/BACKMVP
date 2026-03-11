using MVP.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces;

public interface IIdentityService
{
    Task<List<Usuario>> GetUsuariosActivosAsync();
    Task<(List<Usuario> Items, int TotalCount)> GetUsuariosActivosPagedAsync(int pageNumber, int pageSize);
    Task<Usuario?> GetUsuarioActivoByUidAsync(Guid uid);
    Task<ApplicationResult> CrearUsuarioAsync(Usuario usuario, string password, List<string> rolesNombres);
    Task<ApplicationResult> ActualizarUsuarioAsync(Usuario usuario, List<string> rolesNombres);
    Task<ApplicationResult> BorrarUsuarioAsync(Guid uid);
    Task<List<Rol>> GetRolesByIdsAsync(IEnumerable<int> roleIds);
    Task<List<Rol>> GetRolesByUidsAsync(IEnumerable<Guid> roleUids);
    Task<List<Rol>> GetRolesActivosAsync();
    Task<ApplicationResult> CrearRolAsync(Rol rol);
    Task<bool> RolExisteAsync(string rolNombre);
}

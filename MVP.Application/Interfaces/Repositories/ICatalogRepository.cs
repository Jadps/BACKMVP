using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces.Repositories;

public interface ICatalogRepository
{
    Task<List<Module>> GetActiveModulesWithSubModulesAsync(CancellationToken cancellationToken);
    Task<List<Guid>> GetAllowedModuleIdsForUserAsync(Guid userUid, CancellationToken cancellationToken);
    Task<Guid?> GetUserUidByLoginIdAsync(string loginId, CancellationToken cancellationToken);
    Task<List<Module>> GetModulesByUidsAsync(IEnumerable<Guid> uids, CancellationToken cancellationToken);
    Task<Role?> GetRoleWithPermissionsByUidAsync(Guid uid, CancellationToken cancellationToken);
    void AddRolePermissions(IEnumerable<RoleModule> roleModules);
    void DeleteRolePermissions(IEnumerable<RoleModule> roleModules);
}

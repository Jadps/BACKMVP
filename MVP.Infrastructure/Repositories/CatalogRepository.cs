using Microsoft.EntityFrameworkCore;
using MVP.Application.Interfaces.Repositories;
using MVP.Domain.Entities;
using MVP.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Repositories;

public class CatalogRepository(ApplicationDbContext context) : ICatalogRepository
{
    public async Task<List<Module>> GetActiveModulesWithSubModulesAsync(CancellationToken cancellationToken)
    {
        return await context.Modules
            .AsNoTracking()
            .Include(m => m.SubModules.Where(sm => sm.IsProduction).OrderBy(sm => sm.Order))
            .Where(x => x.IsProduction)
            .OrderBy(m => m.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> GetAllowedModuleIdsForUserAsync(Guid userUid, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.Uid == userUid)
            .Join(context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => ur.RoleId)
            .Join(context.Roles, roleId => roleId, r => r.Id, (roleId, r) => r)
            .SelectMany(r => r.RoleModules)
            .Where(rm => rm.Permission > PermissionLevel.None)
            .Select(rm => rm.ModuleId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}

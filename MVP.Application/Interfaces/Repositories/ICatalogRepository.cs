using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces.Repositories;

public interface ICatalogRepository
{
    Task<List<Module>> GetActiveModulesWithSubModulesAsync(CancellationToken cancellationToken);
    Task<List<int>> GetAllowedModuleIdsForUserAsync(Guid userUid, CancellationToken cancellationToken);
}

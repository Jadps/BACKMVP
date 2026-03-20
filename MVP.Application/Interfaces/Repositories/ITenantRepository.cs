using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces.Repositories;

public interface ITenantRepository
{
    Task<List<Tenant>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<Tenant?> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

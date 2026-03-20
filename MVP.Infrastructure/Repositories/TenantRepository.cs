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

public class TenantRepository(ApplicationDbContext context) : ITenantRepository
{
    public async Task<List<Tenant>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.Tenants
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant?> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default)
    {
        return await context.Tenants
            .FirstOrDefaultAsync(t => t.Uid == uid, cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await context.Tenants.AddAsync(tenant, cancellationToken);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        context.Tenants.Update(tenant);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}

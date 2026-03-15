using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MVP.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MVP.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Module> Modules { get; }
    DbSet<RoleModule> RoleModules { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<FileEntity> Files { get; }
    DbSet<Document> Documents { get; }
    
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<IdentityUserRole<int>> UserRoles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

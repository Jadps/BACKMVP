using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MVP.Domain.Entities;

namespace MVP.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Modulo> Modulos { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Archivo> Archivos { get; }
    DbSet<Documento> Documentos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

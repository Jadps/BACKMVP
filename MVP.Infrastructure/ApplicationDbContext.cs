using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using MVP.Application.Interfaces;
using MVP.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.EntityFrameworkCore.Storage;
using MVP.Domain.Interfaces;

namespace MVP.Infrastructure.Persistence;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>, IApplicationDbContext
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IServiceProvider serviceProvider) : base(options) 
    { 
        _serviceProvider = serviceProvider;
    }

    private int? CurrentTenantId => _serviceProvider.GetService<ICurrentTenantService>()?.TenantId;
    private string? CurrentUserId => _serviceProvider.GetService<ICurrentTenantService>()?.UserId;


    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Archivo> Archivos => Set<Archivo>();
    public DbSet<Documento> Documentos => Set<Documento>();

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
    }

    private void ApplySoftDelete()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete softDelete)
            {
                entry.State = EntityState.Modified;
                softDelete.Borrado = true;
            }
        }
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry);
            auditEntry.TableName = entry.Entity.GetType().Name;
            auditEntry.UserId = CurrentUserId; 
            auditEntry.TenantId = CurrentTenantId;
            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue!;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.AuditType = "Create";
                        auditEntry.NewValues[propertyName] = property.CurrentValue!;
                        break;

                    case EntityState.Deleted:
                        auditEntry.AuditType = "Delete";
                        auditEntry.OldValues[propertyName] = property.OriginalValue!;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.AuditType = "Update";
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                        }
                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
        {
            AuditLogs.Add(auditEntry.ToAudit());
        }

        return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
    }

    private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return Task.CompletedTask;

        foreach (var auditEntry in auditEntries)
        {
            foreach (var prop in auditEntry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue!;
                }
                else
                {
                    auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue!;
                }
            }
            AuditLogs.Add(auditEntry.ToAudit());
        }

        return base.SaveChangesAsync();
    }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Modulo>(entity =>
    {
        entity.ToTable("Modulos");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Uid).IsUnique();
        
        entity.HasOne(m => m.Padre)
              .WithMany(m => m.SubModulos)
              .HasForeignKey(m => m.PadreId)
              .OnDelete(DeleteBehavior.Restrict);
    });

    modelBuilder.Entity<RolModulo>(entity =>
    {
        entity.ToTable("RolesModulos");
        entity.HasKey(rm => new { rm.RolId, rm.ModuloId }); 

        entity.HasOne(rm => rm.Rol)
              .WithMany(r => r.PermisosModulos)
              .HasForeignKey(rm => rm.RolId);

        entity.HasOne(rm => rm.Modulo)
              .WithMany()
              .HasForeignKey(rm => rm.ModuloId);
    });

    modelBuilder.Entity<ApplicationUser>(entity => {
        entity.ToTable("Usuarios");
        entity.HasIndex(e => e.Uid).IsUnique();
        
        entity.HasOne(u => u.Tenant)
              .WithMany()
              .HasForeignKey(u => u.TenantId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(u => u.UserRoles)
              .WithOne()
              .HasForeignKey(ur => ur.UserId)
              .IsRequired();

        entity.HasIndex(u => u.RefreshToken);

        entity.HasQueryFilter(u => !u.Borrado && (CurrentTenantId == null || u.TenantId == CurrentTenantId || u.TenantId == null));
    });
    
    modelBuilder.Entity<Tenant>(entity => {
        entity.ToTable("Tenants");
        entity.HasKey(t => t.Id);
        entity.HasIndex(t => t.Uid).IsUnique();
        entity.HasQueryFilter(t => !t.Borrado && (CurrentTenantId == null || t.Id == CurrentTenantId));
    });
    
    modelBuilder.Entity<ApplicationRole>(entity => {
        entity.ToTable("Roles");
        entity.HasIndex(e => e.Uid).IsUnique();
        entity.HasOne(r => r.Tenant)
              .WithMany()
              .HasForeignKey(r => r.TenantId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasQueryFilter(r => !r.Borrado && (CurrentTenantId == null || r.TenantId == CurrentTenantId || r.TenantId == null));
    });
    modelBuilder.Entity<AuditLog>(entity => {
        entity.ToTable("AuditLogs");
        entity.HasKey(a => a.Id);
        entity.HasIndex(a => a.Uid).IsUnique();
        entity.HasQueryFilter(a => CurrentTenantId == null || a.TenantId == CurrentTenantId);
    });

    modelBuilder.Entity<Archivo>(entity => {
        entity.ToTable("Archivos");
        entity.HasKey(a => a.Id);
        entity.HasIndex(a => a.Uid).IsUnique();
        entity.HasOne(a => a.Tenant)
              .WithMany()
              .HasForeignKey(a => a.TenantId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasQueryFilter(a => !a.Borrado && (CurrentTenantId == null || a.TenantId == CurrentTenantId || a.TenantId == null));
    });

    modelBuilder.Entity<Documento>(entity => {
        entity.ToTable("Documentos");
        entity.HasKey(d => d.Id);
        entity.HasIndex(d => d.Uid).IsUnique();
        
        entity.HasOne(d => d.Archivo)
              .WithMany(a => a.Documentos)
              .HasForeignKey(d => d.ArchivoId)
              .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne(d => d.Tenant)
              .WithMany()
              .HasForeignKey(d => d.TenantId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasQueryFilter(d => !d.Borrado && (CurrentTenantId == null || d.TenantId == CurrentTenantId || d.TenantId == null));
    });

    modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UsuariosRoles");
    modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UsuariosClaims");
    modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UsuariosLogins");
    modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RolesClaims");
    modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UsuariosTokens");
}
}
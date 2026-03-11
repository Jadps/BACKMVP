using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using MVP.Application.Interfaces;
using MVP.Infrastructure.Identity;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace MVP.Infrastructure.Persistence;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    private readonly int? _tenantId;
    private readonly string? _userId;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantService currentTenantService) : base(options) 
    { 
        _tenantId = currentTenantService.TenantId;
        _userId = currentTenantService.UserId;
    }

    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Archivo> Archivos => Set<Archivo>();
    public DbSet<Documento> Documentos => Set<Documento>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
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
            auditEntry.UserId = _userId; 
            auditEntry.TenantId = _tenantId;
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
        
        entity.HasOne(u => u.Tenant)
              .WithMany()
              .HasForeignKey(u => u.TenantId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(u => u.UserRoles)
              .WithOne()
              .HasForeignKey(ur => ur.UserId)
              .IsRequired();

        entity.HasQueryFilter(u => !u.Borrado && (_tenantId == null || u.TenantId == _tenantId || u.TenantId == null));
    });
    
    modelBuilder.Entity<Tenant>(entity => {
        entity.ToTable("Tenants");
        entity.HasKey(t => t.Id);
        entity.HasQueryFilter(t => !t.Borrado && (_tenantId == null || t.Id == _tenantId));
    });
    
    modelBuilder.Entity<ApplicationRole>(entity => {
        entity.ToTable("Roles");
        entity.HasOne(r => r.Tenant)
              .WithMany()
              .HasForeignKey(r => r.TenantId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasQueryFilter(r => !r.Borrado && (_tenantId == null || r.TenantId == _tenantId || r.TenantId == null));
    });
    modelBuilder.Entity<AuditLog>(entity => {
        entity.ToTable("AuditLogs");
        entity.HasKey(a => a.Id);
        entity.HasQueryFilter(a => _tenantId == null || a.TenantId == _tenantId);
    });

    modelBuilder.Entity<Archivo>(entity => {
        entity.ToTable("Archivos");
        entity.HasKey(a => a.Id);
        entity.HasOne(a => a.Tenant)
              .WithMany()
              .HasForeignKey(a => a.TenantId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasQueryFilter(a => !a.Borrado && (_tenantId == null || a.TenantId == _tenantId || a.TenantId == null));
    });

    modelBuilder.Entity<Documento>(entity => {
        entity.ToTable("Documentos");
        entity.HasKey(d => d.Id);
        
        entity.HasOne(d => d.Archivo)
              .WithMany(a => a.Documentos)
              .HasForeignKey(d => d.ArchivoId)
              .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne(d => d.Tenant)
              .WithMany()
              .HasForeignKey(d => d.TenantId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasQueryFilter(d => !d.Borrado && (_tenantId == null || d.TenantId == _tenantId || d.TenantId == null));
    });

    modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UsuariosRoles");
    modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UsuariosClaims");
    modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UsuariosLogins");
    modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RolesClaims");
    modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UsuariosTokens");
}
}
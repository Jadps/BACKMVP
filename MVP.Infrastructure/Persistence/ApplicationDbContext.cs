using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using MVP.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore.Storage;
using MVP.Domain.Interfaces;

namespace MVP.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, int>
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

    public DbSet<Module> Modules => Set<Module>();
    public DbSet<RoleModule> RoleModules => Set<RoleModule>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FileEntity> Files => Set<FileEntity>();
    public DbSet<Document> Documents => Set<Document>();

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
                softDelete.IsDeleted = true;
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

        modelBuilder.Entity<Module>(entity =>
        {
            entity.ToTable("Modules");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Uid).IsUnique();
            
            entity.HasOne(m => m.Parent)
                  .WithMany(m => m.SubModules)
                  .HasForeignKey(m => m.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RoleModule>(entity =>
        {
            entity.ToTable("RoleModules");
            entity.HasKey(rm => new { rm.RoleId, rm.ModuleId }); 

            entity.HasOne(rm => rm.Role)
                  .WithMany(r => r.RoleModules)
                  .HasForeignKey(rm => rm.RoleId);

            entity.HasOne(rm => rm.Module)
                  .WithMany()
                  .HasForeignKey(rm => rm.ModuleId);
        });

        modelBuilder.Entity<User>(entity => {
            entity.ToTable("Users");
            entity.HasIndex(e => e.Uid).IsUnique();
            
            entity.HasOne(u => u.Tenant)
                  .WithMany()
                  .HasForeignKey(u => u.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(u => u.RefreshToken);

            entity.HasQueryFilter(u => !u.IsDeleted && (CurrentTenantId == null || u.TenantId == CurrentTenantId || u.TenantId == null));
        });
        
        modelBuilder.Entity<Tenant>(entity => {
            entity.ToTable("Tenants");
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Uid).IsUnique();
            entity.HasQueryFilter(t => !t.IsDeleted && (CurrentTenantId == null || t.Id == CurrentTenantId));
        });
        
        modelBuilder.Entity<Role>(entity => {
            entity.ToTable("Roles");
            entity.HasIndex(e => e.Uid).IsUnique();
            entity.HasOne(r => r.Tenant)
                  .WithMany()
                  .HasForeignKey(r => r.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(r => !r.IsDeleted && (CurrentTenantId == null || r.TenantId == CurrentTenantId || r.TenantId == null));
        });

        modelBuilder.Entity<AuditLog>(entity => {
            entity.ToTable("AuditLogs");
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.Uid).IsUnique();
            entity.HasQueryFilter(a => CurrentTenantId == null || a.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<FileEntity>(entity => {
            entity.ToTable("Files");
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.Uid).IsUnique();
            entity.HasOne(a => a.Tenant)
                  .WithMany()
                  .HasForeignKey(a => a.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(a => !a.IsDeleted && (CurrentTenantId == null || a.TenantId == CurrentTenantId || a.TenantId == null));
        });

        modelBuilder.Entity<Document>(entity => {
            entity.ToTable("Documents");
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => d.Uid).IsUnique();
            
            entity.HasOne(d => d.File)
                  .WithMany(a => a.Documents)
                  .HasForeignKey(d => d.FileId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Tenant)
                  .WithMany()
                  .HasForeignKey(d => d.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(d => !d.IsDeleted && (CurrentTenantId == null || d.TenantId == CurrentTenantId || d.TenantId == null));
        });

        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
    }
}

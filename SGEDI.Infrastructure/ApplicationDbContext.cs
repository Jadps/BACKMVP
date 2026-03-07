using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGEDI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
namespace SGEDI.Infrastructure.Persistence;
public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

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

    modelBuilder.Entity<Usuario>(entity => {
        entity.ToTable("Usuarios");
        
        entity.HasOne(u => u.Tenant)
              .WithMany(t => t.Usuarios)
              .HasForeignKey(u => u.TenantId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(u => u.UserRoles)
              .WithOne()
              .HasForeignKey(ur => ur.UserId)
              .IsRequired();

        entity.HasQueryFilter(u => !u.Borrado);
    });
    
    modelBuilder.Entity<Tenant>(entity => {
        entity.ToTable("Tenants");
        entity.HasKey(t => t.Id);
        entity.HasQueryFilter(t => !t.Borrado);
    });
    
    modelBuilder.Entity<Rol>(entity => {
        entity.ToTable("Roles");
        entity.HasOne(r => r.Tenant)
              .WithMany(t => t.Roles)
              .HasForeignKey(r => r.TenantId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasQueryFilter(r => !r.Borrado);
    });
    modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UsuariosRoles");
    modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UsuariosClaims");
    modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UsuariosLogins");
    modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RolesClaims");
    modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UsuariosTokens");
}
}
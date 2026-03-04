using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGEDI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
namespace SGEDI.Infrastructure.Persistence;
public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Modulo> Modulos => Set<Modulo>();

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
        entity.HasOne(u => u.Rol).WithMany()
              .HasForeignKey(u => u.RolId)
              .OnDelete(DeleteBehavior.Restrict);
    });
    
    modelBuilder.Entity<Rol>().ToTable("Roles");
    modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UsuariosRoles");
    modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UsuariosClaims");
    modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UsuariosLogins");
    modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RolesClaims");
    modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UsuariosTokens");
}
}
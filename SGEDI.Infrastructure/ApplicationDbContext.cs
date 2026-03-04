using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGEDI.Domain.Entities;

namespace SGEDI.Infrastructure.Persistence;

// Especificamos <Usuario, Rol, string> para que coincida con tus entidades
public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Modulo> Modulos => Set<Modulo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ¡Súper importante! Sin esto, Identity no crea sus tablas base
        base.OnModelCreating(modelBuilder);

        // Configuramos la tabla de Módulos (Jerarquía)
        modelBuilder.Entity<Modulo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(m => m.Padre)
                  .WithMany(m => m.SubModulos)
                  .HasForeignKey(m => m.PadreId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Aquí es donde mapeamos a la Vista que mencionaste (VW_Usuario)
        // para que EF sepa que NO debe intentar crear una tabla con ese nombre
        modelBuilder.Entity<Usuario>()
            .ToTable("Usuarios"); // Cambia el nombre feo 'AspNetUsers' por 'Usuarios'
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MVP.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitialiseDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Rol>>();
            
            await SeedAsync(userManager, roleManager, context);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static async Task SeedAsync(UserManager<Usuario> userManager, RoleManager<Rol> roleManager, ApplicationDbContext context)
    {        
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Nombre == "Empresa Principal");
        if (tenant == null)
        {
            tenant = new Tenant 
            { 
                Nombre = "Empresa Principal", 
                Uid = Guid.NewGuid(),
                FechaCreacion = DateTime.UtcNow,
                Borrado = false
            };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
        }

        if (await roleManager.FindByNameAsync("Admin") == null)
        {
            await roleManager.CreateAsync(new Rol 
            { 
                Name = "Admin", 
                NormalizedName = "ADMIN", 
                TenantId = null, 
                Uid = Guid.NewGuid(),
                Borrado = false
            });
        }
        if (await userManager.FindByEmailAsync("jadp.xs@gmail.com") == null)
        {
            var user = new Usuario
            {
                UserName = "jadp.xs@gmail.com",
                Email = "jadp.xs@gmail.com",
                Nombre = "Alonso",
                PrimerApellido = "Admin",
                FriendlyName = "Alonso Admin",
                TenantId = null, 
                EmailConfirmed = true,
                Uid = Guid.NewGuid(),
                Borrado = false,
                CatStatusAccountId = 1
            };

            var result = await userManager.CreateAsync(user, "Sgedi.2024!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            }
        }
    }
}

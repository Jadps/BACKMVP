using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MVP.Domain.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
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

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            
            await SeedAsync(userManager, roleManager, context);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static async Task SeedAsync(UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context)
    {        
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Name == "Main Company");
        if (tenant == null)
        {
            tenant = new()
            { 
                Name = "Main Company", 
                Uid = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
        }

        if (!await context.Modules.AnyAsync())
        {
            var baseModules = new List<Module>
            {
                new() { Uid = Guid.NewGuid(), Description = "Dashboard", Action = "/dashboard", Icon = "pi pi-home", Order = 1, ModuleTypeId = 1 },
                new() { Uid = Guid.NewGuid(), Description = "Companies", Action = "/tenants", Icon = "pi pi-building", Order = 2, ModuleTypeId = 1 },
                new() { Uid = Guid.NewGuid(), Description = "Users", Action = "/users", Icon = "pi pi-users", Order = 3, ModuleTypeId = 1 },
                new() { Uid = Guid.NewGuid(), Description = "Roles & Permissions", Action = "/roles", Icon = "pi pi-id-card", Order = 4, ModuleTypeId = 1 }
            };
            context.Modules.AddRange(baseModules);
            await context.SaveChangesAsync();
        }

        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            adminRole = new()
            { 
                Name = "Admin", 
                TenantId = null, 
                Uid = Guid.NewGuid(),
                IsDeleted = false
            };
            await roleManager.CreateAsync(adminRole);
        }

        var modulesDb = await context.Modules.ToListAsync();
        foreach (var module in modulesDb)
        {
            var permissionExists = await context.Set<RoleModule>()
                .AnyAsync(rm => rm.RoleId == adminRole.Id && rm.ModuleId == module.Id);

            if (!permissionExists)
            {
                context.Set<RoleModule>().Add(new()
                {
                    RoleId = adminRole.Id,
                    ModuleId = module.Id,
                    Permission = PermissionLevel.Admin
                });
            }
        }
        await context.SaveChangesAsync();

        if (await userManager.FindByEmailAsync("jadp.xs@gmail.com") == null)
        {
            var appUser = new User
            {
                UserName = "jadp.xs@gmail.com",
                Email = "jadp.xs@gmail.com",
                FirstName = "Jesus Alonso",
                LastName = "Dominguez",
                SecondLastName = "Perez",
                FriendlyName = "JADP",
                TenantId = null, 
                EmailConfirmed = true,
                Uid = Guid.NewGuid(),
                IsDeleted = false,
                CatStatusAccountId = 1
            };

            var result = await userManager.CreateAsync(appUser, "Sgedi.2024!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(appUser, "Admin");
            }
        }
    }
}

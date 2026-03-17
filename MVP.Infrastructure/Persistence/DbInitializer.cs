using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MVP.Domain.Entities;
using MVP.Domain.Constants;
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
                new() { Uid = Guid.NewGuid(), Description = "Dashboard", Action = "/dashboard", Icon = "pi pi-home", Order = 1, ModuleTypeId = 1},
                new() { Uid = Guid.NewGuid(), Description = "Administration", Action = "/administration", Icon = "pi pi-cog", Order = 2, ModuleTypeId = 1},
                new() { Uid = Guid.NewGuid(), Description = "Companies", Action = "/tenants", Icon = "pi pi-building", Order = 1, ModuleTypeId = 2, ParentId = 2},
                new() { Uid = Guid.NewGuid(), Description = "Users", Action = "/users", Icon = "pi pi-users", Order = 2, ModuleTypeId = 2, ParentId = 2},
                new() { Uid = Guid.NewGuid(), Description = "Roles & Permissions", Action = "/roles", Icon = "pi pi-id-card", Order = 3, ModuleTypeId = 2, ParentId = 2},
                new() { Uid = Guid.NewGuid(), Description = "Contracts", Action = "/contracts", Icon = "pi pi-file", Order = 4, ModuleTypeId = 1, IsProduction = false}
            };
            context.Modules.AddRange(baseModules);
            await context.SaveChangesAsync();
        }

        var adminRole = await roleManager.FindByNameAsync(AppRoles.GlobalAdmin);
        if (adminRole == null)
        {
            adminRole = new()
            { 
                Name = AppRoles.GlobalAdmin, 
                TenantId = null, 
                Uid = Guid.NewGuid(),
                IsDeleted = false
            };
            await roleManager.CreateAsync(adminRole);
        }

        var userRole = await roleManager.FindByNameAsync(AppRoles.User);
        if (userRole == null)
        {
            userRole = new()
            {
                Name = AppRoles.User,
                TenantId = null,
                Uid = Guid.NewGuid(),
                IsDeleted = false
            };
            await roleManager.CreateAsync(userRole);
        }

        var modulesDb = await context.Modules.ToListAsync();
        foreach (var module in modulesDb)
        {
            var adminPermissionExists = await context.Set<RoleModule>()
                .AnyAsync(rm => rm.RoleId == adminRole.Id && rm.ModuleId == module.Id);

            if (!adminPermissionExists)
            {
                context.Set<RoleModule>().Add(new()
                {
                    RoleId = adminRole.Id,
                    ModuleId = module.Id,
                    Permission = PermissionLevel.Admin
                });
            }

            var userPermissionExists = await context.Set<RoleModule>()
                .AnyAsync(rm => rm.RoleId == userRole.Id && rm.ModuleId == module.Id);

            if (!userPermissionExists)
            {
                context.Set<RoleModule>().Add(new()
                {
                    RoleId = userRole.Id,
                    ModuleId = module.Id,
                    Permission = PermissionLevel.Read
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

            var adminPassword = Environment.GetEnvironmentVariable("AdminInitialPassword");
            if (string.IsNullOrEmpty(adminPassword))
            {
                adminPassword = "Sgedi.2024!"; 
            }

            var result = await userManager.CreateAsync(appUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(appUser, AppRoles.GlobalAdmin);
            }
        }

        if (await userManager.FindByEmailAsync("demo@mail.com") == null)
        {
            var demoUser = new User
            {
                UserName = "demo@mail.com",
                Email = "demo@mail.com",
                FirstName = "Demo",
                LastName = "User",
                FriendlyName = "Demo",
                TenantId = tenant.Id,
                EmailConfirmed = true,
                Uid = Guid.NewGuid(),
                IsDeleted = false,
                CatStatusAccountId = 1
            };

            var result = await userManager.CreateAsync(demoUser, "Demo.2026!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(demoUser, AppRoles.User);
            }
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MVP.Domain.Entities;
using MVP.Domain.Constants;
using MVP.Application.Interfaces;
using MVP.Application.DTOs;
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
            
            await SeedAsync(userManager, roleManager, context, scope.ServiceProvider);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static async Task SeedAsync(UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context, IServiceProvider serviceProvider)
    {
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

        var onboardingService = serviceProvider.GetRequiredService<IOnboardingService>();
        
        var adminEmail = "jadp.xs@gmail.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminPassword = Environment.GetEnvironmentVariable("AdminInitialPassword") ?? "Sgedi.2024!";
            
            var request = new OnboardingRequestDto
            {
                CompanyName = "Main Company",
                Domain = "main.sgedi.com",
                AdminEmail = adminEmail,
                AdminFirstName = "Jesus Alonso",
                AdminLastName = "Dominguez",
                AdminSecondLastName = "Perez",
                AdminPassword = adminPassword
            };

            await onboardingService.RegisterNewTenantAsync(request, AppRoles.GlobalAdmin, isHost: true);
        }

        var demoEmail = "demo@mail.com";
        if (await userManager.FindByEmailAsync(demoEmail) == null)
        {
            var tenant = await context.Tenants.FirstAsync(t => t.Name == "Main Company");
            
            var demoUser = new User
            {
                UserName = demoEmail,
                Email = demoEmail,
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
                var userRoleName = AppRoles.TenantUser;
                if (!await roleManager.RoleExistsAsync(userRoleName))
                {
                    await roleManager.CreateAsync(new Role { Name = userRoleName, TenantId = tenant.Id });
                }
                await userManager.AddToRoleAsync(demoUser, userRoleName);
            }
        }
    }
}

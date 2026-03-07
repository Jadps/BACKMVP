using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SGEDI.Infrastructure.Persistence;
using SGEDI.Domain.Entities;
using SGEDI.Domain.Cifrado;
using Microsoft.AspNetCore.Identity;

namespace SGEDI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));


        services.AddIdentityCore<Usuario>(options => {
            options.Password.RequireDigit = false;
        })
        .AddRoles<Rol>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        var secretKey = configuration["Config:SecretKey"]
            ?? throw new InvalidOperationException("Config:SecretKey no está configurada.");
        services.AddSingleton(new CifradoOptions(secretKey));
        services.AddSingleton<ICifradoService, CifradoService>();

        services.AddScoped(typeof(SGEDI.Domain.Interfaces.IRepository<>), typeof(SGEDI.Infrastructure.Repositories.Repository<>));

        services.AddScoped<SGEDI.Application.Interfaces.IIdentityService, SGEDI.Infrastructure.Services.IdentityService>();

        services.AddScoped<SGEDI.Domain.Interfaces.IUnitOfWork, UnitOfWork>();

        services.AddScoped<SGEDI.Application.Interfaces.Catalogos.IGenericCatalogService, Services.Catalogos.GenericCatalogService>();

        var providerType = typeof(SGEDI.Application.Interfaces.Catalogos.ICatalogoProvider);
        var implementations = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(t => providerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var implementation in implementations)
        {
            services.AddScoped(providerType, implementation);
        }

        return services;
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using MVP.Infrastructure.Persistence;
using MVP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Hangfire;
using Hangfire.PostgreSql;
using MVP.Application.Interfaces.Repositories;
using MVP.Infrastructure.Repositories;

namespace MVP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddAutoMapper(config => {}, Assembly.GetExecutingAssembly());

        services.AddIdentityCore<User>(options => {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<MVP.Application.Interfaces.IUnitOfWork, UnitOfWork>();

        services.Configure<MVP.Infrastructure.Configuration.SftpStorageOptions>(
            configuration.GetSection(MVP.Infrastructure.Configuration.SftpStorageOptions.SectionName));
        services.Configure<MVP.Infrastructure.Configuration.SmtpEmailOptions>(
            configuration.GetSection(MVP.Infrastructure.Configuration.SmtpEmailOptions.SectionName));
        services.Configure<MVP.Infrastructure.Configuration.AppOptions>(
            configuration.GetSection(MVP.Infrastructure.Configuration.AppOptions.SectionName));

        services.AddScoped<MVP.Application.Interfaces.IFileService, MVP.Infrastructure.Services.FileService>();
        services.AddScoped<MVP.Application.Interfaces.IFileStorageService, MVP.Infrastructure.Services.SftpStorageService>();
        services.AddScoped<MVP.Application.Interfaces.IEmailService, MVP.Infrastructure.Services.SmtpEmailService>();
        services.AddScoped<MVP.Application.Interfaces.IIdentityService, MVP.Infrastructure.Services.IdentityService>();
        services.AddScoped<MVP.Application.Interfaces.IAuthService, MVP.Infrastructure.Services.AuthService>();
        services.AddScoped<MVP.Application.Interfaces.Catalogs.IGenericCatalogService, MVP.Infrastructure.Services.Catalogs.GenericCatalogService>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();


        var providerType = typeof(MVP.Application.Interfaces.Catalogs.ICatalogProvider);
        var implementations = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => providerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var implementation in implementations)
        {
            services.AddScoped(providerType, implementation);
        }

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))
            ));

        services.AddHangfireServer();

        return services;
    }
}

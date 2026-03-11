using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MVP.Infrastructure.Persistence;
using MVP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Hangfire;
using Hangfire.PostgreSql;
using MVP.Infrastructure.Identity;
using Microsoft.Extensions.Http.Resilience;

namespace MVP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddAutoMapper(config => {}, Assembly.GetExecutingAssembly());


        services.AddIdentityCore<ApplicationUser>(options => {
            options.Password.RequireDigit = false;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();



        services.AddScoped<MVP.Application.Interfaces.IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.Configure<MVP.Infrastructure.Configuration.SftpStorageOptions>(configuration.GetSection(MVP.Infrastructure.Configuration.SftpStorageOptions.SectionName));
        services.AddScoped<MVP.Application.Interfaces.IFileStorageService, MVP.Infrastructure.Services.SftpStorageService>();
        services.AddScoped<MVP.Application.Interfaces.IArchivoService, MVP.Infrastructure.Services.ArchivoService>();

        services.Configure<MVP.Infrastructure.Configuration.SmtpEmailOptions>(configuration.GetSection(MVP.Infrastructure.Configuration.SmtpEmailOptions.SectionName));
        services.AddScoped<MVP.Application.Interfaces.IEmailService, MVP.Infrastructure.Services.SmtpEmailService>();

        services.Configure<MVP.Infrastructure.Configuration.JwtOptions>(configuration.GetSection(MVP.Infrastructure.Configuration.JwtOptions.SectionName));
        services.Configure<MVP.Infrastructure.Configuration.AppOptions>(configuration.GetSection(MVP.Infrastructure.Configuration.AppOptions.SectionName));

        services.AddScoped<MVP.Application.Interfaces.IIdentityService, MVP.Infrastructure.Services.IdentityService>();
        services.AddScoped<MVP.Application.Interfaces.IAuthService, MVP.Infrastructure.Services.AuthService>();

        services.AddScoped<MVP.Application.Interfaces.Catalogos.IGenericCatalogService, Services.Catalogos.GenericCatalogService>();

        var providerType = typeof(MVP.Application.Interfaces.Catalogos.ICatalogoProvider);
        var implementations = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(t => providerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var implementation in implementations)
        {
            services.AddScoped(providerType, implementation);
        }

        services.AddHttpClient("ExternalApi")
            .AddStandardResilienceHandler();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(name: "database", tags: ["db", "sql"]);

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
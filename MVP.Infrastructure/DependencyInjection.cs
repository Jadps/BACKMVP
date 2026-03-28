using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MVP.Infrastructure.Configuration;
using System;
using System.Linq;
using System.Reflection;
using MVP.Infrastructure.Persistence;
using MVP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Hangfire;
using Hangfire.PostgreSql;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Repositories;
using MVP.Infrastructure.Repositories;
using MVP.Infrastructure.Services;

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

        services.Configure<MVP.Infrastructure.Configuration.SmtpEmailOptions>(
            configuration.GetSection(MVP.Infrastructure.Configuration.SmtpEmailOptions.SectionName));
        services.Configure<MVP.Infrastructure.Configuration.AppOptions>(
            configuration.GetSection(MVP.Infrastructure.Configuration.AppOptions.SectionName));

        services.AddScoped<MVP.Application.Interfaces.IFileService, MVP.Infrastructure.Services.FileService>();
        services.AddScoped<MVP.Application.Interfaces.IFileStorageService, MVP.Infrastructure.Services.SupabaseStorageService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IPdfGeneratorService, GotenbergPdfService>();
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

        services.AddHttpClient<IPdfGeneratorService, GotenbergPdfService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<AppOptions>>().Value;
            client.BaseAddress = new Uri(options.GotenbergUrl);
        });

        var supabaseUrl = configuration["Supabase:Url"];
        var supabaseKey = configuration["Supabase:Key"];
        if (!string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseKey))
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };
            services.AddScoped<Supabase.Client>(_ => new Supabase.Client(supabaseUrl, supabaseKey, options));
        }
        services.AddScoped<MVP.Infrastructure.Services.ISupabaseStorageService, MVP.Infrastructure.Services.SupabaseStorageService>();

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

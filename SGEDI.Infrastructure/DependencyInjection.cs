using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SGEDI.Infrastructure.Persistence;
using SGEDI.Domain.Entities;
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

        services.AddScoped(typeof(SGEDI.Domain.Interfaces.IRepository<>), typeof(SGEDI.Infrastructure.Repositories.Repository<>));

        return services;
    }
}
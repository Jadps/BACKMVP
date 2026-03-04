using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace SGEDI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Aquí configuraremos el DbContext más adelante
        // services.AddDbContext<ApplicationDbContext>(options =>
        //    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}
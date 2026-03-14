using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;
using FluentValidation;
using MVP.Application.Behaviors;
using MediatR;

namespace MVP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(config => {}, Assembly.GetExecutingAssembly());
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddScoped<Interfaces.Users.IUserService, Services.Users.UserService>();
        services.AddScoped<Interfaces.Catalogos.ICatalogService, Services.Catalogos.CatalogService>();
        services.AddScoped<Interfaces.ITenantService, Services.TenantService>();
        services.AddScoped<Interfaces.IOnboardingService, Services.OnboardingService>();

        return services;
    }
}

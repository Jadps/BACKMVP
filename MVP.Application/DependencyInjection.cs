using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;

namespace MVP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        services.AddAutoMapper(config => {}, Assembly.GetExecutingAssembly());
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(Behaviors.ValidationBehavior<,>));
        });

        services.AddScoped<Interfaces.Usuarios.IUsuarioService, Services.Usuarios.UsuarioService>();
        
        services.AddScoped<Interfaces.Catalogos.ICatalogoService, Services.Catalogos.CatalogoService>();
        
        services.AddScoped<Interfaces.ITenantService, Services.TenantService>();

        services.AddScoped<Interfaces.IOnboardingService, Services.OnboardingService>();

        return services;
    }
}
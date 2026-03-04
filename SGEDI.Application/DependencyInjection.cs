using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SGEDI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        services.AddAutoMapper(config => {}, Assembly.GetExecutingAssembly());

        services.AddScoped<Interfaces.Usuarios.IUsuarioService, Services.Usuarios.UsuarioService>();
        
        services.AddScoped<Interfaces.Catalogos.ICatalogoService, Services.Catalogos.CatalogoService>();
        
        services.AddScoped<SGEDI.Domain.Cifrado.ICifradoService, CifradoService>();

        return services;
    }
}
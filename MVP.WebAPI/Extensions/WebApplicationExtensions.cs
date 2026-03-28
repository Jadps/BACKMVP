using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace MVP.WebAPI.Extensions;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseAntiforgeryTokenMiddleware(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
            
            var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
            var cookieDomain = configuration["Config:CookieDomain"] ?? ".alonsodev.online";

            var tokens = antiforgery.GetAndStoreTokens(context);

            if (tokens.RequestToken != null)
            {
                context.Response.Cookies.Append(
                    "XSRF-TOKEN",
                    tokens.RequestToken,
                    new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Domain = cookieDomain
                    });
            }

            if (HttpMethods.IsPost(context.Request.Method) ||
                HttpMethods.IsPut(context.Request.Method) ||
                HttpMethods.IsDelete(context.Request.Method))
            {
                var endpoint = context.GetEndpoint();
                var ignoreAntiforgery = endpoint?.Metadata.GetMetadata<IgnoreAntiforgeryTokenAttribute>();

                if (ignoreAntiforgery == null && !context.Request.Path.StartsWithSegments("/hubs"))
                {
                    try
                    {
                        await antiforgery.ValidateRequestAsync(context);
                    }
                    catch (AntiforgeryValidationException)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new { message = "Antiforgery token validation failed." });
                        return;
                    }
                }
            }

            await next();
        });
    }
}
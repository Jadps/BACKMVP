using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace MVP.WebAPI.Extensions;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseAntiforgeryTokenMiddleware(this IApplicationBuilder app)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var cookieDomain = configuration["Config:CookieDomain"] ?? null;

        return app.Use(async (context, next) =>
        {
            var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
            
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

            await next();
        });
    }
}
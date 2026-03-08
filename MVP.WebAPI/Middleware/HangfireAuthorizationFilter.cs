using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace MVP.WebAPI.Middleware;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext.Request.Host.Host.StartsWith("localhost") || httpContext.Request.Host.Host.StartsWith("127.0.0.1"))
        {
            return true;
        }
        return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin");
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Repositories;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace MVP.WebAPI.Middleware;

public class TenantResolverMiddleware(RequestDelegate next, ILogger<TenantResolverMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ICurrentTenantService currentTenantService, IMemoryCache cache, ITenantRepository tenantRepository)
    {
        IDisposable? tenantContext = null;
        IDisposable? userContext = null;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userEmail = context.User.Identity.Name;
            userContext = LogContext.PushProperty("UserEmail", userEmail);

            var tenantClaim = context.User.FindFirst("TenantId")?.Value;
            if (Guid.TryParse(tenantClaim, out Guid tenantUid))
            {
                var cacheKey = $"tenant_id_{tenantUid}";
                if (!cache.TryGetValue(cacheKey, out int tenantId))
                {
                    var tenant = await tenantRepository.GetByUidAsync(tenantUid);

                    if (tenant != null)
                    {
                        tenantId = tenant.Id;
                        cache.Set(cacheKey, tenantId, TimeSpan.FromMinutes(60));
                    }
                }

                if (tenantId != 0)
                {
                    currentTenantService.SetTenantId(tenantId);
                    tenantContext = LogContext.PushProperty("TenantId", tenantUid);
                }
                else
                {
                    logger.LogWarning("Tenant with UID {TenantUid} not found in database for authenticated user {UserEmail}", tenantUid, context.User.Identity?.Name);
                }
            }
            else
            {
                logger.LogWarning("TenantId claim not found or invalid for authenticated user {UserEmail}", context.User.Identity?.Name);
            }
        }

        try
        {
            await next(context);
        }
        finally
        {
            tenantContext?.Dispose();
            userContext?.Dispose();
        }
    }
}

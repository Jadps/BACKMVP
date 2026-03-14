using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using MVP.Application.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVP.WebAPI.Middleware;

public class TenantResolverMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICurrentTenantService currentTenantService, IMemoryCache cache, IApplicationDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("TenantId")?.Value;
            if (Guid.TryParse(tenantClaim, out Guid tenantUid))
            {
                var cacheKey = $"tenant_id_{tenantUid}";
                if (!cache.TryGetValue(cacheKey, out int tenantId))
                {
                    var tenant = await dbContext.Tenants
                        .AsNoTracking()
                        .Where(t => t.Uid == tenantUid)
                        .Select(t => new { t.Id })
                        .FirstOrDefaultAsync();

                    if (tenant != null)
                    {
                        tenantId = tenant.Id;
                        cache.Set(cacheKey, tenantId, TimeSpan.FromMinutes(60));
                    }
                }

                if (tenantId != 0)
                {
                    currentTenantService.SetTenantId(tenantId);
                }
            }
        }

        await next(context);
    }
}

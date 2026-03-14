using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using MVP.Application.Interfaces;
using MVP.Domain.Constants;
using System.Security.Claims;
using System;
using System.Linq;

namespace MVP.WebAPI.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private int? _tenantId;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? TenantId
    {
        get
        {
            if (IsSuperAdmin) return null;
            return _tenantId;
        }
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public bool IsSuperAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole(AppRoles.GlobalAdmin) ?? false;

    public void SetTenantId(int tenantId)
    {
        _tenantId = tenantId;
    }
}

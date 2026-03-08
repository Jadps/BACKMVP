using Microsoft.AspNetCore.Http;
using SGEDI.Application.Interfaces;
using System.Security.Claims;

namespace SGEDI.WebAPI.Services;

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
            if (_tenantId.HasValue) return _tenantId.Value;

            var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;
            if (int.TryParse(tenantClaim, out int tenantId))
            {
                return tenantId;
            }
            return null;
        }
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public bool IsSuperAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("SuperAdmin") ?? false;

    public void SetTenantId(int tenantId)
    {
        _tenantId = tenantId;
    }
}

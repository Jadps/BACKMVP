namespace MVP.Application.Interfaces;

public interface ICurrentTenantService
{
    int? TenantId { get; }
    string? UserId { get; }
    bool IsSuperAdmin { get; }
    void SetTenantId(int tenantId);
}

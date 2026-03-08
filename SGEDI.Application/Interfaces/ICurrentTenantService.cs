namespace SGEDI.Application.Interfaces;

public interface ICurrentTenantService
{
    int? TenantId { get; }
    string? UserId { get; }
    void SetTenantId(int tenantId);
}

namespace SGEDI.Application.Interfaces;

public interface ICurrentTenantService
{
    int? TenantId { get; }
    void SetTenantId(int tenantId);
}

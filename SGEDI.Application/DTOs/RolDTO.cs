namespace SGEDI.Application.DTOs;

public class RolDTO
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? TenantId { get; set; }
}
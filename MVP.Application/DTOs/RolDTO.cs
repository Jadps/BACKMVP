using System;

namespace MVP.Application.DTOs;

public class RolDTO
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public Guid? TenantId { get; set; }
}
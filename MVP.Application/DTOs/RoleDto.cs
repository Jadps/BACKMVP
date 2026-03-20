using System;

namespace MVP.Application.DTOs;

public class RoleDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? TenantId { get; set; }
    public List<RolePermissionDto> Permissions { get; set; } = new();
}

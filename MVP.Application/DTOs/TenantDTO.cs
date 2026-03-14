using System;

namespace MVP.Application.DTOs;

public class TenantDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

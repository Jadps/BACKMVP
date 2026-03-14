using System;

namespace MVP.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string? UserId { get; set; }
    public int? TenantId { get; set; }
    public string AuditType { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedColumns { get; set; }
    public string PrimaryKey { get; set; } = string.Empty;
}

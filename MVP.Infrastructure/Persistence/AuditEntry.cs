using Microsoft.EntityFrameworkCore.ChangeTracking;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MVP.Infrastructure.Persistence;

public class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }
    public string? UserId { get; set; }
    public int? TenantId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, object> KeyValues { get; } = new();
    public Dictionary<string, object> OldValues { get; } = new();
    public Dictionary<string, object> NewValues { get; } = new();
    public List<PropertyEntry> TemporaryProperties { get; } = new();
    public string AuditType { get; set; } = string.Empty;
    public List<string> ChangedColumns { get; } = new();

    public bool HasTemporaryProperties => TemporaryProperties.Any();

    public AuditLog ToAudit()
    {
        var audit = new AuditLog
        {
            UserId = UserId,
            TenantId = TenantId,
            AuditType = AuditType,
            TableName = TableName,
            CreatedAt = DateTime.UtcNow,
            PrimaryKey = JsonSerializer.Serialize(KeyValues),
            OldValues = OldValues.Any() ? JsonSerializer.Serialize(OldValues) : null,
            NewValues = NewValues.Any() ? JsonSerializer.Serialize(NewValues) : null,
            ChangedColumns = ChangedColumns.Any() ? JsonSerializer.Serialize(ChangedColumns) : null
        };
        return audit;
    }
}

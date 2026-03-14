using System;
using MVP.Domain.Interfaces;

namespace MVP.Domain.Entities;

public enum ValidationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    NotApplicable = 3
}

public class Document : ISoftDelete
{
    public int Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();

    public string DocumentType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    public int? FileId { get; set; }
    public virtual FileEntity? File { get; set; }

    public ValidationStatus ValidationStatus { get; set; } = ValidationStatus.Pending;
    public string? RejectionObservations { get; set; }

    public int? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

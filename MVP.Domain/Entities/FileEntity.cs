using System;
using System.Collections.Generic;
using MVP.Domain.Interfaces;

namespace MVP.Domain.Entities;

public class FileEntity : ISoftDelete
{
    public int Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    
    public string OriginalName { get; set; } = string.Empty;
    public string PhysicalPath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    public int? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}

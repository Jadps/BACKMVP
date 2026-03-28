using System;
using MVP.Domain.Interfaces;

namespace MVP.Domain.Entities;

public class GeneratedReport : ISoftDelete
{
    public int Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    
    public int UserId { get; set; }
    public virtual User? User { get; set; }

    public string ReportType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    
    public int? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

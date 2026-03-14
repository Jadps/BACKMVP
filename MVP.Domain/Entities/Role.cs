using Microsoft.AspNetCore.Identity;
using MVP.Domain.Interfaces;

namespace MVP.Domain.Entities;

public class Role : IdentityRole<int>, ISoftDelete
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    
    public int? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    public virtual ICollection<RoleModule> RoleModules { get; set; } = new List<RoleModule>();
}

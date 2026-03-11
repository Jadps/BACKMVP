using Microsoft.AspNetCore.Identity;
using MVP.Domain.Entities;

namespace MVP.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string? Descripcion { get; set; }
    public bool Borrado { get; set; }
    public int? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    public virtual ICollection<RolModulo> PermisosModulos { get; set; } = new List<RolModulo>();
}

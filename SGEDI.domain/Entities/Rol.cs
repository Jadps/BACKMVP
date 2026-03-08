using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Identity;

using SGEDI.Domain.Interfaces;

namespace SGEDI.Domain.Entities;

public class Rol : IdentityRole<int>, ISoftDelete
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string? Descripcion { get; set; }
    public bool Borrado { get; set; }
    public int? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    public virtual ICollection<RolModulo> PermisosModulos { get; set; } = new List<RolModulo>();
}


using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Identity;

namespace SGEDI.Domain.Entities;

public class Rol : IdentityRole<int>
{
    public string? Descripcion { get; set; }
    public bool Borrado { get; set; }
    public virtual ICollection<RolModulo> PermisosModulos { get; set; } = new List<RolModulo>();
}


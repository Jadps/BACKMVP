using System;
using System.Collections.Generic;

namespace SGEDI.Domain.Entities;

public class Tenant
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Dominio { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public virtual ICollection<Rol> Roles { get; set; } = new List<Rol>();
}

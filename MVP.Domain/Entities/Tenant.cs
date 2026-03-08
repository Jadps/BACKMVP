using System;
using System.Collections.Generic;

using MVP.Domain.Interfaces;

namespace MVP.Domain.Entities;

public class Tenant : ISoftDelete
{
    public int Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string? Dominio { get; set; }
    public bool Borrado { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public virtual ICollection<Rol> Roles { get; set; } = new List<Rol>();
}

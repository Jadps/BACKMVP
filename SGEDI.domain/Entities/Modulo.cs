using System;
using System.Collections.Generic;
using System.Text;

namespace SGEDI.Domain.Entities;

public class Modulo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Ruta { get; set; }
    public string? Icono { get; set; }

    // Para la jerarquía de submódulos
    public int? PadreId { get; set; }
    public Modulo? Padre { get; set; }
    public ICollection<Modulo> SubModulos { get; set; } = new List<Modulo>();

    // Relación con Roles (Muchos a Muchos)
    public ICollection<Rol> Roles { get; set; } = new List<Rol>();
}
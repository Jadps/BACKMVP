using System;
using System.Collections.Generic;
using System.Text;

namespace SGEDI.Domain.Entities;

public class Modulo
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty; 
    public string? Accion { get; set; } 
    public string? Icono { get; set; }
    public int? Orden { get; set; }
    public bool Produccion { get; set; } = true;
    public int? PadreId { get; set; }
    public virtual Modulo? Padre { get; set; }
    public virtual ICollection<Modulo> SubModulos { get; set; } = new List<Modulo>();
    public int TipoModuloId { get; set; } 
}
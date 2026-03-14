using System;
using System.Collections.Generic;

namespace MVP.Application.DTOs;

public class ModuloDTO
{
    public Guid? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public string? Accion { get; set; }
    public int Orden { get; set; }
    public Guid? PadreId { get; set; }
    public List<ModuloDTO> SubModulos { get; set; } = new();
}
using System;

namespace MVP.Application.DTOs;

public class TenantDTO
{
    public Guid? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Dominio { get; set; }
    public bool Borrado { get; set; }
    public DateTime FechaCreacion { get; set; }
}

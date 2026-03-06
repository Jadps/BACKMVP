using System;

namespace SGEDI.Application.DTOs;

public class TenantDTO
{
    public string? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Dominio { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

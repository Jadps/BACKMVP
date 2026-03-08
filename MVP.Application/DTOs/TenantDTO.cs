using System;
using System.ComponentModel.DataAnnotations;

namespace MVP.Application.DTOs;

public class TenantDTO
{
    public Guid? Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string? Dominio { get; set; }
    
    public bool Borrado { get; set; }
    public DateTime FechaCreacion { get; set; }
}

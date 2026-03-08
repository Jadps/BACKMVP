using System;
using MVP.Domain.Interfaces;

namespace MVP.Domain.Entities;

public enum EstadoValidacion
{
    Pendiente = 0,
    Aprobado = 1,
    Rechazado = 2,
    NoAplica = 3
}

public class Documento : ISoftDelete
{
    public int Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();

    public string TipoDocumento { get; set; } = string.Empty;
    public string EntidadTipo { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;

    public int? ArchivoId { get; set; }
    public virtual Archivo? Archivo { get; set; }

    public EstadoValidacion EstadoValidacion { get; set; } = EstadoValidacion.Pendiente;
    public string? ObservacionesRechazo { get; set; }

    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    
    public bool Borrado { get; set; } = false;
    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
}

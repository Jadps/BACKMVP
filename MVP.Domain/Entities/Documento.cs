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

    // The required document type (e.g., 'IdentificacionOficial', 'ContratoPrueba')
    public string TipoDocumento { get; set; } = string.Empty;

    // Polymorphic association (to what entity does this required document belong?)
    public string EntidadTipo { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;

    // The actual attached file (Null if the user hasn't uploaded it yet)
    public int? ArchivoId { get; set; }
    public virtual Archivo? Archivo { get; set; }

    // Validation State
    public EstadoValidacion EstadoValidacion { get; set; } = EstadoValidacion.Pendiente;
    public string? ObservacionesRechazo { get; set; }

    // Tenant and Tracking
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    
    public bool Borrado { get; set; } = false;
    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
}

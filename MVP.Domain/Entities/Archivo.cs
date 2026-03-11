using System;
using System.Collections.Generic;
using MVP.Domain.Interfaces;

namespace MVP.Domain.Entities;

public class Archivo : ISoftDelete
{
    public int Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    
    public string NombreOriginal { get; set; } = string.Empty;
    public string RutaFisica { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    
    public string EntidadTipo { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;

    public int? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    
    public bool Borrado { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}

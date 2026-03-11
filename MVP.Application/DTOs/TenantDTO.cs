using System;

namespace MVP.Application.DTOs;

public record TenantDTO(
    Guid? Id,
    string Nombre,
    string? Dominio,
    bool Borrado,
    DateTime FechaCreacion);

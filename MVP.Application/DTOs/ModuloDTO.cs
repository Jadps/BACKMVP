using System;
using System.Collections.Generic;

namespace MVP.Application.DTOs;

public record ModuloDTO(
    Guid? Id,
    string Nombre,
    string? Icono,
    string? Accion,
    int Orden,
    Guid? PadreId,
    List<ModuloDTO> SubModulos);
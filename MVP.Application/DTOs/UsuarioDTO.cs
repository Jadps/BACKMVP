using System;
using System.Collections.Generic;

namespace MVP.Application.DTOs;

public record UsuarioDTO(
    Guid? Id,
    string Email,
    string Nombre,
    string PrimerApellido,
    string? SegundoApellido,
    string? Password,
    int? TenantId,
    List<RolDTO> Roles,
    string? NombreCompleto);

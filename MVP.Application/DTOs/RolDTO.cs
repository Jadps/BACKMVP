using System;

namespace MVP.Application.DTOs;

public record RolDTO(Guid? Id, string Name, string? Descripcion, int? TenantId);
using System;

namespace MVP.Application.DTOs;

public record CatalogoItemDTO(
    Guid Id,
    string Descripcion,
    string? Adicional = null,
    string? Adicional2 = null);

using System;

namespace MVP.Application.DTOs;

public class CatalogoItemDTO
{
    public Guid Id { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public string? Adicional { get; init; }
    public string? Adicional2 { get; init; }
}

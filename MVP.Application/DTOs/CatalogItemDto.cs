using System;

namespace MVP.Application.DTOs;

public class CatalogItemDto
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Additional { get; init; }
    public string? Additional2 { get; init; }
}

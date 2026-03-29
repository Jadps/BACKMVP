using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace MVP.Application.DTOs;

public class ModuleDto
{
    public Guid? Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Action { get; set; }
    public int Order { get; set; }
    public int ModuleTypeId { get; set; }
    public Guid? ParentId { get; set; }
    public List<ModuleDto> SubModules { get; set; } = new();
}

using System;
using System.Collections.Generic;

namespace MVP.Domain.Entities;

public class Module
{
    public int Id { get; set; }
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty; 
    public string? Action { get; set; } 
    public string? Icon { get; set; }
    public int? Order { get; set; }
    public bool IsProduction { get; set; } = true;
    public int? ParentId { get; set; }
    public virtual Module? Parent { get; set; }
    public virtual ICollection<Module> SubModules { get; set; } = new List<Module>();
    public int ModuleTypeId { get; set; } 
}

namespace MVP.Domain.Entities;

public class RoleModule
{
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    public int ModuleId { get; set; }
    public virtual Module Module { get; set; } = null!;

    public PermissionLevel Permission { get; set; }
}

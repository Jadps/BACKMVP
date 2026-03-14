namespace MVP.Domain.Entities;

public enum PermissionLevel
{
    None = 0,
    Read = 1,        // View
    Write = 2,       // View + Create/Edit
    Admin = 3        // View + Create/Edit + SoftDelete
}

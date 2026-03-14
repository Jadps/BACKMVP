using Microsoft.AspNetCore.Identity;
using MVP.Domain.Interfaces;

namespace MVP.Domain.Entities;

public class User : IdentityUser<int>, ISoftDelete
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? SecondLastName { get; set; }
    public string FriendlyName { get; set; } = string.Empty;

    public int? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public int CatStatusAccountId { get; set; }
    public bool IsDeleted { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }

    public List<int>? RoleIds { get; set; }

    public string FullName => $"{FirstName} {LastName} {SecondLastName}".Trim();
}

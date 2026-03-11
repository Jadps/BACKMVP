using Microsoft.AspNetCore.Identity;
using MVP.Domain.Entities;

namespace MVP.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string FriendlyName { get; set; } = string.Empty;
    public int? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    public int CatStatusAccountId { get; set; }
    public bool Borrado { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
    public virtual ICollection<IdentityUserRole<int>> UserRoles { get; set; } = new List<IdentityUserRole<int>>();

    public string NombreCompleto => $"{Nombre} {PrimerApellido} {SegundoApellido}".Trim();
}

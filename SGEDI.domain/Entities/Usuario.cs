using Microsoft.AspNetCore.Identity;

namespace SGEDI.Domain.Entities;

public class Usuario : IdentityUser<int>
{
    public string Nombre { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string FriendlyName { get; set; } = string.Empty;

    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public int CatStatusAccountId { get; set; }
    public bool Borrado { get; set; }
    public string NombreCompleto => $"{Nombre} {PrimerApellido} {SegundoApellido}".Trim();

    public virtual ICollection<IdentityUserRole<int>> UserRoles { get; set; } = new List<IdentityUserRole<int>>();
}
using Microsoft.AspNetCore.Identity;

namespace SGEDI.Domain.Entities;

public class Usuario : IdentityUser<int>
{
    public string Nombre { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string FriendlyName { get; set; } = string.Empty;

    public int? PersonalID { get; set; }
    public int? RolId { get; set; }
    public virtual Rol? Rol { get; set; }
    public int CatStatusAccountId { get; set; }
    public int? UbicacionId { get; set; }

    public bool SuperUser { get; set; }
    public long? ChatId { get; set; }
    public bool Firmante { get; set; }
    public bool Borrado { get; set; }
    public string NombreCompleto => $"{Nombre} {PrimerApellido} {SegundoApellido}".Trim();
}
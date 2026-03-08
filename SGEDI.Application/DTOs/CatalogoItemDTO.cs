namespace SGEDI.Application.DTOs;

public class CatalogoItemDTO
{
    public Guid Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Adicional { get; set; }
    public string? Adicional2 { get; set; }

    public CatalogoItemDTO() { }

    public CatalogoItemDTO(Guid id, string descripcion, string? adicional = null, string? adicional2 = null)
    {
        Id = id;
        Descripcion = descripcion;
        Adicional = adicional;
        Adicional2 = adicional2;
    }
}

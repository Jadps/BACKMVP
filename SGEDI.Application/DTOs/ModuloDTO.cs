namespace SGEDI.Application.DTOs;

public class ModuloDTO
{
    public string? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public string? Url { get; set; }
    public int Orden { get; set; }

    public string? PadreId { get; set; }
    public List<ModuloDTO> SubModulos { get; set; } = new();
}
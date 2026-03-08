namespace SGEDI.Application.DTOs;

public class OnboardingRequestDTO
{
    public string EmpresaNombre { get; set; } = string.Empty;
    public string Dominio { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminNombre { get; set; } = string.Empty;
    public string AdminPrimerApellido { get; set; } = string.Empty;
    public string? AdminSegundoApellido { get; set; }
}

namespace MVP.Application.DTOs;

public class OnboardingRequestDTO
{
    public string EmpresaNombre { get; init; } = string.Empty;
    public string Dominio { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
    public string AdminNombre { get; init; } = string.Empty;
    public string AdminPrimerApellido { get; init; } = string.Empty;
    public string? AdminSegundoApellido { get; init; }
}

namespace MVP.Application.DTOs;

public record OnboardingRequestDTO(
    string EmpresaNombre,
    string Dominio,
    string AdminEmail,
    string AdminPassword,
    string AdminNombre,
    string AdminPrimerApellido,
    string? AdminSegundoApellido);

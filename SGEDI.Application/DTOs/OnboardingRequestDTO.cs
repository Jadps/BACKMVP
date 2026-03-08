using System.ComponentModel.DataAnnotations;

namespace SGEDI.Application.DTOs;

public class OnboardingRequestDTO
{
    [Required]
    [MaxLength(200)]
    public string EmpresaNombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Dominio { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string AdminEmail { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string AdminPassword { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AdminNombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AdminPrimerApellido { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? AdminSegundoApellido { get; set; }
}

namespace MVP.Application.DTOs;

public record OnboardingRequestDto
{
    public string CompanyName { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
    public string AdminFirstName { get; init; } = string.Empty;
    public string AdminLastName { get; init; } = string.Empty;
    public string? AdminSecondLastName { get; init; }
}

namespace MVP.Infrastructure.Configuration;

public record JwtOptions
{
    public const string SectionName = "Jwt"; 

    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpirationInMinutes { get; init; } = 60;
}

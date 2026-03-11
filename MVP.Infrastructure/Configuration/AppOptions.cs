namespace MVP.Infrastructure.Configuration;

public record AppOptions
{
    public const string SectionName = "Config";

    public string FrontendUrl { get; init; } = string.Empty;
}

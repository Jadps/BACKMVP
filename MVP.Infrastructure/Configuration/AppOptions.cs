namespace MVP.Infrastructure.Configuration;

public record AppOptions
{
    public const string SectionName = "Config";

    public string FrontendUrl { get; init; } = string.Empty;
    public string CookieDomain { get; init; } = string.Empty;
    public string AntiforgeryHeaderName { get; init; } = "X-XSRF-TOKEN";
}

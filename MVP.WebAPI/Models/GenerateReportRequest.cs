namespace MVP.WebAPI.Models;

public record GenerateReportRequest
{
    public string ReportType { get; init; } = string.Empty;
}

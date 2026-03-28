namespace MVP.Application.Messages;

public record GenerateReportCommand
{
    public Guid UserId { get; init; }
    public string ReportType { get; init; } = string.Empty;
}

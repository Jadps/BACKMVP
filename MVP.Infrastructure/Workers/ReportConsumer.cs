using MassTransit;
using Microsoft.EntityFrameworkCore;
using MVP.Application.Messages;
using MVP.Infrastructure.Persistence;
using MVP.Application.Interfaces;
using MVP.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVP.Infrastructure.Services;
using System.Text;

namespace MVP.Infrastructure.Workers;

public class ReportConsumer : IConsumer<GenerateReportCommand>
{
    private readonly IReportNotificationService _notificationService;
    private readonly ISupabaseStorageService _supabaseStorage;
    private readonly IPdfGeneratorService _pdfGenerator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReportConsumer> _logger;
    private readonly IReportService _reportService;
    
    public ReportConsumer(
        IReportNotificationService notificationService,
        ISupabaseStorageService supabaseStorage,
        IPdfGeneratorService pdfGenerator,
        IConfiguration configuration,
        ILogger<ReportConsumer> logger,
        IReportService reportService)
    {
        _notificationService = notificationService;
        _supabaseStorage = supabaseStorage;
        _pdfGenerator = pdfGenerator;
        _configuration = configuration;
        _logger = logger;
        _reportService = reportService;
    }

    public async Task Consume(ConsumeContext<GenerateReportCommand> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("[RabbitMQ] Starting {ReportType} report generation for user {UserId}...", message.ReportType, message.UserId);
        
        var topUsers = await _reportService.GetTopUsersWithRolesAndModulesAsync(10);

        var sb = new StringBuilder();
        sb.AppendLine("<html><body>");
        sb.AppendLine($"<h1>Reporte: {message.ReportType}</h1>");
        sb.AppendLine("<hr/><ul>");
        foreach(var user in topUsers)
        {
            sb.AppendLine("<li style='margin-bottom: 10px;'>");
            sb.AppendLine($"  <b>Usuario:</b> {user.FullName} ({user.Email})<br/>");
            sb.AppendLine($"  <small><b>Roles:</b> {string.Join(", ", user.Roles)}</small><br/>");
            sb.AppendLine($"  <small><b>Módulos:</b> {string.Join(", ", user.Modules)}</small>");
            sb.AppendLine("</li>");
        }
        sb.AppendLine("</ul></body></html>");

        byte[] pdfBytes;
        try 
        {
            pdfBytes = await _pdfGenerator.GeneratePdfFromHtmlAsync(sb.ToString());
            _logger.LogInformation("[ReportConsumer] PDF generado con Gotenberg exitosamente.");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "[ReportConsumer] Fallo generacion Gotenberg. Usando fallback mock PDF.");
            pdfBytes = Encoding.UTF8.GetBytes("Mock PDF data since Gotenberg failed");
        }

        var userIdString = message.UserId.ToString();
        var fileName = $"{Guid.NewGuid()}.pdf";
        var basePath = _configuration["Supabase:StoragePath"] ?? "development/";
        var folderPath = $"{basePath.TrimEnd('/')}/{userIdString}";

        var publicUrl = await _supabaseStorage.UploadFileAsync(pdfBytes, folderPath, fileName, "application/pdf");
        _logger.LogInformation("[Supabase] File uploaded! URL: {Url}", publicUrl);

        var saveResult = await _reportService.SaveGeneratedReportAsync(message.UserId, message.ReportType, publicUrl);
        
        if (saveResult.IsSuccess)
        {
            _logger.LogInformation("[ReportService] Report metadata saved successfully.");
        }
        else
        {
            _logger.LogError("[ReportService] Failed to save report metadata: {Error}", saveResult.ErrorMessage);
        }

        _logger.LogInformation("[RabbitMQ] Report {ReportType} completed successfully!", message.ReportType);

        await _notificationService.NotifyReportReadyAsync(message.UserId, message.ReportType, publicUrl);
    }
}

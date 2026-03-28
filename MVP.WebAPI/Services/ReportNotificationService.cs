using Microsoft.AspNetCore.SignalR;
using MVP.Application.Interfaces;
using MVP.WebAPI.Hubs;
using System;
using System.Threading.Tasks;

namespace MVP.WebAPI.Services;

public class ReportNotificationService : IReportNotificationService
{
    private readonly IHubContext<ReportsHub> _hubContext;

    public ReportNotificationService(IHubContext<ReportsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyReportReadyAsync(Guid userId, string reportType, string fileUrl)
    {

        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReportReady", new { 
            ReportType = reportType, 
            FileUrl = fileUrl,
            CompletedAt = DateTime.UtcNow 
        });
    }
}

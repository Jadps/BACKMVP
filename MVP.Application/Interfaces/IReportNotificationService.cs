using System;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces;

public interface IReportNotificationService
{
    Task NotifyReportReadyAsync(Guid userId, string reportType, string fileUrl);
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MVP.Domain.Entities;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces;

public interface IReportService
{
    Task<List<ReportUserDto>> GetTopUsersWithRolesAndModulesAsync(int count);
    Task<ApplicationResult> SaveGeneratedReportAsync(Guid userId, string reportType, string fileUrl);
}

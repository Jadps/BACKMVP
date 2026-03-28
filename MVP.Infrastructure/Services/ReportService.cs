using Microsoft.EntityFrameworkCore;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Domain.Entities;
using MVP.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Services;

public class ReportService(ApplicationDbContext dbContext) : IReportService
{
    public async Task<List<ReportUserDto>> GetTopUsersWithRolesAndModulesAsync(int count)
    {
        return await dbContext.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => dbContext.Roles.Where(r => r.Id == ur.RoleId))
                    .ThenInclude(r => r.RoleModules)
                        .ThenInclude(rm => rm.Module)
            .Take(count)
            .Select(u => new ReportUserDto
            {
                FullName = u.FullName,
                Email = u.Email,
                Roles = u.UserRoles.Select(ur => dbContext.Roles.First(r => r.Id == ur.RoleId).Name!).ToList(),
                Modules = u.UserRoles
                    .SelectMany(ur => dbContext.Roles.First(r => r.Id == ur.RoleId).RoleModules)
                    .Select(rm => rm.Module.Description)
                    .Distinct()
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<ApplicationResult> SaveGeneratedReportAsync(Guid userId, string reportType, string fileUrl)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Uid == userId);
        if (user == null)
            return ApplicationResult.Failure("User not found.");

        var generatedReport = new GeneratedReport
        {
            Uid = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = user.TenantId,
            ReportType = reportType,
            FileUrl = fileUrl,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.GeneratedReports.Add(generatedReport);
        await dbContext.SaveChangesAsync();

        return ApplicationResult.Success();
    }
}

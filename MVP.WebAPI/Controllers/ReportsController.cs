using Asp.Versioning;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MVP.Application.Interfaces;
using MVP.Application.Messages;
using MVP.WebAPI.Models;
using System;
using System.Threading.Tasks;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("StrictPolicy")]
public class ReportsController(
    IPublishEndpoint publishEndpoint,
    ICurrentTenantService currentTenantService) : ControllerBase
{
    [Authorize]
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateReportRequest request)
    {
        var userUidStr = currentTenantService.UserId;
        
        if (string.IsNullOrEmpty(userUidStr) || !Guid.TryParse(userUidStr, out Guid uid))
        {
            return Unauthorized(new { message = "Unauthorized or invalid user ID." });
        }

        var command = new GenerateReportCommand 
        { 
            UserId = uid,
            ReportType = request.ReportType 
        };

        await publishEndpoint.Publish(command);

        return Accepted(new { message = "Your report is being processed in the background." });
    }
}

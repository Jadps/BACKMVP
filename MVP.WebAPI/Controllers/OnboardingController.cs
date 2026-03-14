using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.WebAPI.Extensions;
using System.Threading.Tasks;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class OnboardingController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(OnboardingRequestDto request)
    {
        var result = await onboardingService.RegisterNewTenantAsync(request);
        return result.ToActionResult();
    }
}

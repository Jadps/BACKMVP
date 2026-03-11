using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;

using MVP.WebAPI.Extensions;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[AllowAnonymous]
public class OnboardingController(IOnboardingService onboardingService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] OnboardingRequestDTO request)
    {
        var result = await onboardingService.RegistrarNuevoTenantAsync(request);
        return result.ToActionResult();
    }
}

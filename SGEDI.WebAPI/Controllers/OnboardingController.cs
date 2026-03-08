using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces;

namespace SGEDI.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[AllowAnonymous]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;

    public OnboardingController(IOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] OnboardingRequestDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _onboardingService.RegistrarNuevoTenantAsync(request);
        
        if (result.Succeeded)
        {
            return Ok(new { message = "Se ha registrado correctamente la organización y su administrador." });
        }

        return BadRequest(result.Errors);
    }
}

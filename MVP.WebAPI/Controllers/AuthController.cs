using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using System.Threading.Tasks;
using Asp.Versioning;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO model)
    {
        var result = await _authService.LoginAsync(model);
        
        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        if (result.ErrorType == ErrorType.Unauthorized)
        {
            return Unauthorized(new { message = result.Errors.FirstOrDefault() ?? "No autorizado." });
        }

        return BadRequest(new { errors = result.Errors });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userEmail = User.Identity?.Name;
        var result = await _authService.LogoutAsync(userEmail);
        
        if (result.Succeeded)
        {
            return Ok(new { message = "Sesión cerrada exitosamente (Refresh Token revocado)." });
        }

        return BadRequest(new { errors = result.Errors });
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO model)
    {
        var result = await _authService.RefreshTokenAsync(model);
        
        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        if (result.ErrorType == ErrorType.Unauthorized)
        {
            return Unauthorized(new { message = result.Errors.FirstOrDefault() ?? "No autorizado." });
        }

        return BadRequest(new { errors = result.Errors });
    }
}

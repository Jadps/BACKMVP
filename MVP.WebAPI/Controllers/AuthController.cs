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
        var tokenResponse = await _authService.LoginAsync(model);
        
        if (tokenResponse != null)
        {
            return Ok(tokenResponse);
        }
        return Unauthorized(new { message = "Email o contraseña incorrecta." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userEmail = User.Identity?.Name;
        await _authService.LogoutAsync(userEmail);
        
        return Ok(new { message = "Sesión cerrada exitosamente (Refresh Token revocado)." });
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO model)
    {
        var tokenResponse = await _authService.RefreshTokenAsync(model);
        
        if (tokenResponse != null)
        {
            return Ok(tokenResponse);
        }

        return Unauthorized(new { message = "Refresh Token inválido o expirado." });
    }
}

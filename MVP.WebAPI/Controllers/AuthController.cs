using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.WebAPI.Extensions;
using System.Threading.Tasks;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("StrictPolicy")]
    [HttpPost("login")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Login([FromBody] LoginDTO model)
    {
        var result = await authService.LoginAsync(model);

        if (result.IsSuccess && result.Data is { } authResponse)
        {
            Response.AppendSecureCookie("AccessToken", authResponse.AccessToken!, authResponse!.Expiration);
            Response.AppendSecureCookie("RefreshToken", authResponse.RefreshToken!, DateTime.UtcNow.AddDays(7));

            return Ok(authResponse);
        }

        if (result.ErrorType == ErrorType.Unauthorized)
            return Unauthorized(new { message = result.Errors.FirstOrDefault() ?? "No autorizado." });

        return BadRequest(new { errors = result.Errors });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userEmail = User.Identity?.Name;
        var result = await authService.LogoutAsync(userEmail);

        if (result.IsSuccess)
        {
            Response.DeleteSecureCookie("AccessToken");
            Response.DeleteSecureCookie("RefreshToken");

            return Ok(new { message = "Sesión cerrada exitosamente (Refresh Token revocado)." });
        }

        return BadRequest(new { errors = result.Errors });
    }

    [AllowAnonymous]
    [EnableRateLimiting("StrictPolicy")]
    [HttpPost("refresh-token")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["RefreshToken"];
        
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "No se encontró el token de refresco en las cookies." });

        var model = new RefreshTokenRequestDTO(refreshToken);
        var result = await authService.RefreshTokenAsync(model);

        if (result.IsSuccess && result.Data is { } authResponse)
        {
            Response.AppendSecureCookie("AccessToken", authResponse.AccessToken!, authResponse!.Expiration);
            Response.AppendSecureCookie("RefreshToken", authResponse.RefreshToken!, DateTime.UtcNow.AddDays(7));

            return Ok(authResponse);
        }

        return Unauthorized(new { message = result.Errors.FirstOrDefault() ?? "Token expirado." });
    }

    [AllowAnonymous]
    [EnableRateLimiting("StrictPolicy")]
    [HttpPost("forgot-password")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
    {
        var result = await authService.ForgotPasswordAsync(model);
        
        if (result.IsSuccess)
        {
            return Ok(new { message = "Si el correo está registrado, hemos enviado las instrucciones para restablecer tu contraseña." });
        }

        return BadRequest(new { errors = result.Errors });
    }

    [AllowAnonymous]
    [EnableRateLimiting("StrictPolicy")]
    [HttpPost("reset-password")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
    {
        var result = await authService.ResetPasswordAsync(model);
        
        if (result.IsSuccess)
        {
            return Ok(new { message = "La contraseña ha sido restablecida exitosamente." });
        }

        return BadRequest(new { errors = result.Errors });
    }
}

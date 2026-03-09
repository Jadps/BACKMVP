using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting("StrictPolicy")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO model)
    {
        var result = await _authService.LoginAsync(model);
        
        if (result.Succeeded)
        {
            var authResponse = result.Value;
            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                Expires = authResponse!.Expiration
            };
            Response.Cookies.Append("AccessToken", authResponse.AccessToken!, cookieOptions);

            var refreshCookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                Expires = System.DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("RefreshToken", authResponse.RefreshToken!, refreshCookieOptions);

            return Ok(authResponse);
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
            Response.Cookies.Delete("AccessToken", new Microsoft.AspNetCore.Http.CookieOptions { Secure = true, SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });
            Response.Cookies.Delete("RefreshToken", new Microsoft.AspNetCore.Http.CookieOptions { Secure = true, SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None });
            return Ok(new { message = "Sesión cerrada exitosamente (Refresh Token revocado)." });
        }

        return BadRequest(new { errors = result.Errors });
    }

    [AllowAnonymous]
    [EnableRateLimiting("StrictPolicy")]
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

    [AllowAnonymous]
    [EnableRateLimiting("StrictPolicy")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
    {
        var result = await _authService.ForgotPasswordAsync(model);
        
        if (result.Succeeded)
        {
            return Ok(new { message = "Si el correo está registrado, hemos enviado las instrucciones para restablecer tu contraseña." });
        }

        return BadRequest(new { errors = result.Errors });
    }

    [AllowAnonymous]
    [EnableRateLimiting("StrictPolicy")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
    {
        var result = await _authService.ResetPasswordAsync(model);
        
        if (result.Succeeded)
        {
            return Ok(new { message = "La contraseña ha sido restablecida exitosamente." });
        }

        return BadRequest(new { errors = result.Errors });
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.WebAPI.Extensions;

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
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await authService.LoginAsync(request);

        if (result.IsSuccess && result.Data != null)
        {
            Response.AppendSecureCookie("AccessToken", result.Data.Token, result.Data.Expiration);
            Response.AppendSecureCookie("RefreshToken", result.Data.RefreshToken, DateTime.UtcNow.AddDays(7));

            return Ok(new { message = "Successfully login" });
        }

        return Unauthorized(result.ErrorMessage);
    }

    [Authorize]
    [HttpPost("logout")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userEmail = User.Identity?.Name;
        var result = await authService.LogoutAsync(userEmail);

        if (result.IsSuccess)
        {
            Response.DeleteSecureCookie("AccessToken");
            Response.DeleteSecureCookie("RefreshToken");
            return Ok(new { message = "Successfully logged out" });
        }

        return result.ToActionResult();
    }

    [AllowAnonymous]
    [EnableRateLimiting("StrictPolicy")]
    [HttpPost("refresh-token")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["RefreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token not found in cookies." });

        var request = new RefreshTokenRequestDto(refreshToken);
        var result = await authService.RefreshTokenAsync(request);

        if (result.IsSuccess && result.Data != null)
        {
            Response.AppendSecureCookie("AccessToken", result.Data.Token, result.Data.Expiration);
            Response.AppendSecureCookie("RefreshToken", result.Data.RefreshToken, DateTime.UtcNow.AddDays(7));

            return Ok(new { message = "Token refreshed successfully" });
        }

        return result.ToActionResult();
    }

    [AllowAnonymous]
    [EnableRateLimiting("StrictPolicy")]
    [HttpPost("forgot-password")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        var result = await authService.ForgotPasswordAsync(request);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [EnableRateLimiting("StrictPolicy")]
    [HttpPost("reset-password")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var result = await authService.ResetPasswordAsync(request);
        return result.ToActionResult();
    }
}

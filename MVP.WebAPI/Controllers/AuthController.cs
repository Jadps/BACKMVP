using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MVP.Application.DTOs;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<Usuario> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<Usuario> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO model)
    {
        var user = await _userManager.FindByNameAsync(model.Email);
        
        if (user != null && !user.Borrado && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var tokenResponse = await GenerarYAsignarTokensAsync(user);
            return Ok(tokenResponse);
        }
        return Unauthorized(new { message = "Email o contraseña incorrecta." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userEmail = User.Identity?.Name;
        if (!string.IsNullOrEmpty(userEmail))
        {
            var user = await _userManager.FindByNameAsync(userEmail);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiration = null;
                await _userManager.UpdateAsync(user);
            }
        }
        return Ok(new { message = "Sesión cerrada exitosamente (Refresh Token revocado)." });
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO model)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == model.RefreshToken);
        
        if (user == null || user.Borrado || user.RefreshTokenExpiration <= DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Refresh Token inválido o expirado." });
        }

        var tokenResponse = await GenerarYAsignarTokensAsync(user);
        return Ok(tokenResponse);
    }

    private async Task<object> GenerarYAsignarTokensAsync(Usuario user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.TenantId.HasValue)
        {
            authClaims.Add(new Claim("TenantId", user.TenantId.Value.ToString()));
        }

        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var secretKey = _configuration["Config:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("Error de configuración de seguridad.");
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddMinutes(15),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var refreshToken = Guid.NewGuid().ToString();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(token),
            refreshToken = refreshToken,
            expiration = token.ValidTo
        };
    }
}

public class RefreshTokenRequestDTO
{
    public string RefreshToken { get; set; } = string.Empty;
}

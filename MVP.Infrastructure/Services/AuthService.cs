using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Domain.Entities;

namespace MVP.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(UserManager<Usuario> userManager, IConfiguration configuration, IEmailService emailService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<ApplicationResult<AuthResponseDTO>> LoginAsync(LoginDTO model)
    {
        var user = await _userManager.FindByNameAsync(model.Email);
        
        if (user != null && !user.Borrado && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return ApplicationResult<AuthResponseDTO>.Success(await GenerarYAsignarTokensAsync(user));
        }
        return ApplicationResult<AuthResponseDTO>.Failure(new[] { "Email o contraseña incorrecta." }, ErrorType.Unauthorized);
    }

    public async Task<ApplicationResult> LogoutAsync(string? userEmail)
    {
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
        return ApplicationResult.Success();
    }

    public async Task<ApplicationResult<AuthResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO model)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == model.RefreshToken);
        
        if (user == null || user.Borrado || user.RefreshTokenExpiration <= DateTime.UtcNow)
        {
            return ApplicationResult<AuthResponseDTO>.Failure(new[] { "Refresh Token inválido o expirado." }, ErrorType.Unauthorized);
        }

        return ApplicationResult<AuthResponseDTO>.Success(await GenerarYAsignarTokensAsync(user));
    }

    public async Task<ApplicationResult> ForgotPasswordAsync(ForgotPasswordDTO model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || user.Borrado)
        {
            return ApplicationResult.Success();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        var frontendUrl = _configuration["Config:FrontendUrl"] ?? "http://localhost:4200";
        var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email!)}";

        var subject = "Recuperación de Contraseña - SGEDI";
        var body = $@"
            <h3>Recuperación de Contraseña</h3>
            <p>Hemos recibido una solicitud para cambiar tu contraseña.</p>
            <p>Haz clic en el siguiente enlace para establecer una nueva contraseña:</p>
            <p><a href='{resetLink}'>Recuperar mi contraseña</a></p>
            <br/>
            <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>
        ";

        BackgroundJob.Enqueue<IEmailService>(emailService => emailService.SendEmailAsync(user.Email!, subject, body, true));

        return ApplicationResult.Success();
    }

    public async Task<ApplicationResult> ResetPasswordAsync(ResetPasswordDTO model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || user.Borrado)
        {
            return ApplicationResult.Failure(new[] { "Token o usuario inválido." }, ErrorType.Validation);
        }

        var resetResult = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!resetResult.Succeeded)
        {
            var errors = new List<string>();
            foreach(var err in resetResult.Errors) errors.Add(err.Description);
            return ApplicationResult.Failure(errors, ErrorType.Validation);
        }

        return ApplicationResult.Success();
    }

    private async Task<AuthResponseDTO> GenerarYAsignarTokensAsync(Usuario user)
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

        return new AuthResponseDTO
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            Expiration = token.ValidTo
        };
    }
}

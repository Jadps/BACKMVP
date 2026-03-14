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
using MVP.Infrastructure.Identity;

using Microsoft.Extensions.Options;
using MVP.Infrastructure.Configuration;

namespace MVP.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager, 
    IOptions<JwtOptions> jwtOptions,
    IOptions<AppOptions> appOptions,
    IBackgroundJobClient backgroundJobs,
    IApplicationDbContext context) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;
    private readonly AppOptions _app = appOptions.Value;

    public async Task<ApplicationResult<AuthResponseDTO>> LoginAsync(LoginDTO model)
    {
        var user = await userManager.FindByNameAsync(model.Email);
        
        if (user != null && !user.Borrado && await userManager.CheckPasswordAsync(user, model.Password))
        {
            return ApplicationResult<AuthResponseDTO>.Success(await GenerarYAsignarTokensAsync(user));
        }
        return ApplicationResult<AuthResponseDTO>.Failure("Email o contraseña incorrecta.", ErrorType.Unauthorized);
    }

    public async Task<ApplicationResult> LogoutAsync(string? userEmail)
    {
        if (!string.IsNullOrEmpty(userEmail))
        {
            var user = await userManager.FindByNameAsync(userEmail);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiration = null;
                await userManager.UpdateAsync(user);
            }
        }
        return ApplicationResult.Success();
    }

    public async Task<ApplicationResult<AuthResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO model)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == model.RefreshToken);
        
        if (user == null || user.Borrado || user.RefreshTokenExpiration <= DateTime.UtcNow)
        {
            return ApplicationResult<AuthResponseDTO>.Failure("Refresh Token inválido o expirado.", ErrorType.Unauthorized);
        }

        return ApplicationResult<AuthResponseDTO>.Success(await GenerarYAsignarTokensAsync(user));
    }

    public async Task<ApplicationResult> ForgotPasswordAsync(ForgotPasswordDTO model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null || user.Borrado)
        {
            return ApplicationResult.Success();
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        
        var frontendUrl = _app.FrontendUrl;
        var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email!)}";

        backgroundJobs.Enqueue<IEmailService>(emailService => 
            emailService.SendPasswordResetEmailAsync(user.Email!, resetLink));

        return ApplicationResult.Success();
    }

    public async Task<ApplicationResult> ResetPasswordAsync(ResetPasswordDTO model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null || user.Borrado)
        {
            return ApplicationResult.Failure("Token o usuario inválido.", ErrorType.Validation);
        }

        var resetResult = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!resetResult.Succeeded)
        {
            return ApplicationResult.Failure(resetResult.Errors.Select(e => e.Description), ErrorType.Validation);
        }

        return ApplicationResult.Success();
    }

    private async Task<AuthResponseDTO> GenerarYAsignarTokensAsync(ApplicationUser user)
    {
        var userRoles = await userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Uid.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.TenantId.HasValue)
        {
            var tenant = await context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == user.TenantId.Value);
            if (tenant != null)
            {
                authClaims.Add(new Claim("TenantId", tenant.Uid.ToString()));
            }
        }

        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var secretKey = _jwt.SecretKey;
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("Error de configuración de seguridad.");
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpirationInMinutes),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var refreshToken = Guid.NewGuid().ToString();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        return new AuthResponseDTO(
            new JwtSecurityTokenHandler().WriteToken(token),
            refreshToken,
            token.ValidTo
        );
    }
}

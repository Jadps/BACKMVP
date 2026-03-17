using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Domain.Entities;
using MVP.Infrastructure.Configuration;

namespace MVP.Infrastructure.Services;

public class AuthService(
    UserManager<User> userManager, 
    IOptions<JwtOptions> jwtOptions,
    IOptions<AppOptions> appOptions,
    IBackgroundJobClient backgroundJobs,
    IApplicationDbContext context) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;
    private readonly AppOptions _app = appOptions.Value;

    public async Task<ApplicationResult<AuthResponseDto>> LoginAsync(LoginDto model)
    {
        var user = await userManager.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.NormalizedUserName == userManager.NormalizeName(model.Email));
        
        if (user != null && !user.IsDeleted && await userManager.CheckPasswordAsync(user, model.Password))
        {
            return ApplicationResult<AuthResponseDto>.Success(await GenerateAndAssignTokensAsync(user, isRefresh: false));
        }
        return ApplicationResult<AuthResponseDto>.Failure("Invalid email or password.", ErrorType.Unauthorized);
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

    public async Task<ApplicationResult<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto model)
    {
        var user = await userManager.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.RefreshToken == model.RefreshToken);
        
        if (user == null || user.IsDeleted || user.RefreshTokenExpiration <= DateTime.UtcNow)
        {
            return ApplicationResult<AuthResponseDto>.Failure("Invalid or expired Refresh Token.", ErrorType.Unauthorized);
        }

        return ApplicationResult<AuthResponseDto>.Success(await GenerateAndAssignTokensAsync(user, isRefresh: true));
    }

    public async Task<ApplicationResult> ForgotPasswordAsync(ForgotPasswordDto model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null || user.IsDeleted)
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

    public async Task<ApplicationResult> ResetPasswordAsync(ResetPasswordDto model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null || user.IsDeleted)
        {
            return ApplicationResult.Failure("Invalid token or user.", ErrorType.Validation);
        }

        var resetResult = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!resetResult.Succeeded)
        {
            return ApplicationResult.Failure(resetResult.Errors.Select(e => e.Description), ErrorType.Validation);
        }

        return ApplicationResult.Success();
    }

    private async Task<AuthResponseDto> GenerateAndAssignTokensAsync(User user, bool isRefresh)
    {
        var userRoles = await userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Uid.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.TenantId.HasValue && user.Tenant != null)
        {
            authClaims.Add(new Claim("TenantId", user.Tenant.Uid.ToString()));
        }

        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var secretKey = _jwt.SecretKey;
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("Security configuration error: SecretKey is missing.");
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
        
        if (!isRefresh)
        {
            user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(7);
        }

        await userManager.UpdateAsync(user);

        return new AuthResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            refreshToken,
            token.ValidTo
        );
    }
}

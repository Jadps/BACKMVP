using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Domain.Constants;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;

namespace MVP.Application.Services;

public class OnboardingService(
    IApplicationDbContext context, 
    IIdentityService identityService, 
    IBackgroundJobClient backgroundJobs) : IOnboardingService
{
    public async Task<ApplicationResult> RegisterNewTenantAsync(OnboardingRequestDto request)
    {
        using var transaction = await context.BeginTransactionAsync();
        try
        {
            if (!await identityService.RoleExistsAsync(AppRoles.TenantAdmin))
            {
                await identityService.CreateRoleAsync(new Role { Name = AppRoles.TenantAdmin });
            }

            var tenant = new Tenant
            {
                Uid = Guid.NewGuid(),
                Name = request.CompanyName,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await context.Tenants.AddAsync(tenant);
            await context.SaveChangesAsync();

            var adminUser = new User
            {
                Uid = Guid.NewGuid(),
                FirstName = request.AdminFirstName,
                LastName = request.AdminLastName,
                Email = request.AdminEmail,
                UserName = request.AdminEmail,
                TenantId = tenant.Id,
                CatStatusAccountId = 1,
                IsDeleted = false
            };

            var userResult = await identityService.CreateUserAsync(adminUser, request.AdminPassword, new List<string> { AppRoles.TenantAdmin });
            if (!userResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return userResult;
            }

            await transaction.CommitAsync();

            backgroundJobs.Enqueue<IEmailService>(emailService => 
                emailService.SendWelcomeEmailAsync(adminUser.Email!, adminUser.FirstName));

            return ApplicationResult.Success();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

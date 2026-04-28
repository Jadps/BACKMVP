using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Repositories;
using MVP.Domain.Constants;
using MVP.Domain.Entities;
using MVP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace MVP.Application.Services;

public class OnboardingService(
    IUnitOfWork unitOfWork,
    ITenantRepository tenantRepository,
    ICatalogRepository catalogRepository,
    IIdentityService identityService, 
    IBackgroundJobClient backgroundJobs,
    ILogger<OnboardingService> logger) : IOnboardingService
{
    public async Task<ApplicationResult> RegisterNewTenantAsync(OnboardingRequestDto request, string initialRoleName = AppRoles.TenantAdmin, bool isHost = false)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var tenant = new Tenant
            {
                Uid = Guid.NewGuid(),
                Name = request.CompanyName,
                Domain = request.Domain,
                IsHost = isHost,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await tenantRepository.AddAsync(tenant);
            await unitOfWork.SaveChangesAsync(); 

            var role = new Role 
            { 
                Name = initialRoleName,
                TenantId = tenant.Id,
                Uid = Guid.NewGuid(),
                IsDeleted = false
            };

            var modules = await catalogRepository.GetActiveModulesWithSubModulesAsync(default);
            
            foreach (var parent in modules)
            {
                AddModulePermission(role, parent);
                if (parent.SubModules != null)
                {
                    foreach (var sub in parent.SubModules)
                    {
                        AddModulePermission(role, sub);
                    }
                }
            }

            var roleResult = await identityService.CreateRoleAsync(role);
            if (!roleResult.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync();
                return roleResult;
            }

            var adminUser = new User
            {
                Uid = Guid.NewGuid(),
                FirstName = request.AdminFirstName,
                LastName = request.AdminLastName,
                SecondLastName = request.AdminSecondLastName,
                Email = request.AdminEmail,
                UserName = request.AdminEmail,
                TenantId = tenant.Id,
                CatStatusAccountId = (int)UserAccountStatus.Active,
                IsDeleted = false
            };

            var userResult = await identityService.CreateUserAsync(adminUser, request.AdminPassword, new List<string> { initialRoleName });
            if (!userResult.IsSuccess)
            {
                await unitOfWork.RollbackTransactionAsync();
                return userResult;
            }

            await unitOfWork.CommitTransactionAsync();

            backgroundJobs.Enqueue<IEmailService>(emailService => 
                emailService.SendWelcomeEmailAsync(adminUser.Email!, adminUser.FirstName));

            return ApplicationResult.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during tenant onboarding for company {CompanyName}", request.CompanyName);
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private void AddModulePermission(Role role, Module module)
    {
        if (!role.RoleModules.Any(rm => rm.ModuleId == module.Id))
        {
            role.RoleModules.Add(new RoleModule
            {
                ModuleId = module.Id,
                Permission = PermissionLevel.Admin
            });
        }
    }
}

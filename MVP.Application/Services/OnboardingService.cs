using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Domain.Constants;
using MVP.Domain.Entities;
using MVP.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;

namespace MVP.Application.Services;

public class OnboardingService(
    IApplicationDbContext context, 
    IIdentityService identityService, 
    IMapper mapper,
    IBackgroundJobClient backgroundJobs) : IOnboardingService
{
    public async Task<ApplicationResult> RegistrarNuevoTenantAsync(OnboardingRequestDTO request)
    {
        using var transaction = await context.BeginTransactionAsync();
        try
        {
            if (!await identityService.RolExisteAsync(AppRoles.TenantAdmin))
            {
                await identityService.CrearRolAsync(new Rol { Name = AppRoles.TenantAdmin });
            }

            var tenant = mapper.Map<Tenant>(request);
            await context.Tenants.AddAsync(tenant);
            await context.SaveChangesAsync();

            var adminUser = mapper.Map<Usuario>(request);
            adminUser.TenantId = tenant.Id;

            var userResult = await identityService.CrearUsuarioAsync(adminUser, request.AdminPassword, new List<string> { AppRoles.TenantAdmin });
            if (!userResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return userResult;
            }

            await transaction.CommitAsync();

            backgroundJobs.Enqueue<IEmailService>(emailService => 
                emailService.SendWelcomeEmailAsync(adminUser.Email!, adminUser.Nombre));

            return ApplicationResult.Success();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

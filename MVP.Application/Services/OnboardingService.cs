using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Domain.Constants;
using MVP.Domain.Entities;
using MVP.Domain.Interfaces;

namespace MVP.Application.Services;

public class OnboardingService : IOnboardingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public OnboardingService(IUnitOfWork unitOfWork, IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<ApplicationResult> RegistrarNuevoTenantAsync(OnboardingRequestDTO request)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (!await _identityService.RolExisteAsync(AppRoles.TenantAdmin))
            {
                await _identityService.CrearRolAsync(new Rol 
                { 
                    Name = AppRoles.TenantAdmin 
                });
            }

            var tenant = new Tenant
            {
                Nombre = request.EmpresaNombre,
                Dominio = request.Dominio,
                FechaCreacion = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Tenant>().AddAsync(tenant);
            await _unitOfWork.CommitAsync();

            var adminUser = new Usuario
            {
                UserName = request.AdminEmail,
                Email = request.AdminEmail,
                Nombre = request.AdminNombre,
                PrimerApellido = request.AdminPrimerApellido,
                SegundoApellido = request.AdminSegundoApellido,
                FriendlyName = request.AdminNombre,
                TenantId = tenant.Id
            };

            var userResult = await _identityService.CrearUsuarioAsync(adminUser, request.AdminPassword, new List<string> { AppRoles.TenantAdmin });
            if (!userResult.Succeeded)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return userResult;
            }

            await _unitOfWork.CommitTransactionAsync();
            return ApplicationResult.Success();
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}

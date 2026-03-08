using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces;
using SGEDI.Domain.Entities;
using SGEDI.Domain.Interfaces;

namespace SGEDI.Application.Services;

public class OnboardingService : IOnboardingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<Usuario> _userManager;
    private readonly RoleManager<Rol> _roleManager;

    public OnboardingService(IUnitOfWork unitOfWork, UserManager<Usuario> userManager, RoleManager<Rol> roleManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ApplicationResult> RegistrarNuevoTenantAsync(OnboardingRequestDTO request)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 1. Ensure TenantAdmin role exists globally
            if (!await _roleManager.RoleExistsAsync("TenantAdmin"))
            {
                await _roleManager.CreateAsync(new Rol { Name = "TenantAdmin", NormalizedName = "TENANTADMIN" });
            }

            // 2. Create Tenant
            var tenant = new Tenant
            {
                Nombre = request.EmpresaNombre,
                Dominio = request.Dominio,
                FechaCreacion = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Tenant>().AddAsync(tenant);
            await _unitOfWork.CommitAsync(); // Gets the Id generated

            // 3. Create initial Admin user
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

            var userResult = await _userManager.CreateAsync(adminUser, request.AdminPassword);
            if (!userResult.Succeeded)
            {
                await _unitOfWork.RollbackTransactionAsync();
                var errors = new List<string>();
                foreach (var err in userResult.Errors) errors.Add(err.Description);
                return ApplicationResult.Failure(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(adminUser, "TenantAdmin");
            if (!roleResult.Succeeded)
            {
                await _unitOfWork.RollbackTransactionAsync();
                var errors = new List<string>();
                foreach (var err in roleResult.Errors) errors.Add(err.Description);
                return ApplicationResult.Failure(errors);
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

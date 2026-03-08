using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGEDI.Application.Interfaces;
using SGEDI.Domain.Entities;
using SGEDI.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGEDI.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly RoleManager<Rol> _roleManager;
    private readonly IUnitOfWork _unitOfWork;

    public IdentityService(UserManager<Usuario> userManager, RoleManager<Rol> roleManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Usuario>> GetUsuariosActivosAsync()
    {
        return await _userManager.Users
            .Where(u => !u.Borrado)
            .Include(u => u.UserRoles)
            .ToListAsync();
    }

    public async Task<Usuario?> GetUsuarioActivoByUidAsync(Guid uid)
    {
        return await _userManager.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Uid == uid && !u.Borrado);
    }

    public async Task<ApplicationResult> CrearUsuarioAsync(Usuario usuario, string password, List<string> rolesNombres)
    {
        await _unitOfWork.BeginTransactionAsync();
        try 
        {
            var result = await _userManager.CreateAsync(usuario, password);

            if (result.Succeeded && rolesNombres.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(usuario, rolesNombres);
                if (!roleResult.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApplicationResult.Failure(roleResult.Errors.Select(e => e.Description));
                }
            }

            if (result.Succeeded)
            {
                await _unitOfWork.CommitTransactionAsync();
                return ApplicationResult.Success();
            }

            await _unitOfWork.RollbackTransactionAsync();
            return ApplicationResult.Failure(result.Errors.Select(e => e.Description));
        }
        catch 
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ApplicationResult> ActualizarUsuarioAsync(Usuario usuario, List<string> rolesNombres)
    {
        await _unitOfWork.BeginTransactionAsync();
        try 
        {
            var result = await _userManager.UpdateAsync(usuario);

            if (result.Succeeded)
            {
                var rolesActuales = await _userManager.GetRolesAsync(usuario);
                if (rolesActuales.Any())
                {
                    await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
                }

                if (rolesNombres.Any())
                {
                    await _userManager.AddToRolesAsync(usuario, rolesNombres);
                }

                await _unitOfWork.CommitTransactionAsync();
                return ApplicationResult.Success();
            }

            await _unitOfWork.RollbackTransactionAsync();
            return ApplicationResult.Failure(result.Errors.Select(e => e.Description));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> BorrarUsuarioAsync(Guid uid)
    {
        var user = await GetUsuarioActivoByUidAsync(uid);
        if (user == null) return false;

        user.Borrado = true;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<List<Rol>> GetRolesByIdsAsync(IEnumerable<int> roleIds)
    {
        return await _roleManager.Roles
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync();
    }

    public async Task<List<Rol>> GetRolesByUidsAsync(IEnumerable<Guid> roleUids)
    {
        return await _roleManager.Roles
            .Where(r => roleUids.Contains(r.Uid))
            .ToListAsync();
    }
}

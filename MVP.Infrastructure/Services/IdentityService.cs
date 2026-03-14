using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVP.Application.Interfaces;
using MVP.Domain.Entities;
using MVP.Domain.Interfaces;
using MVP.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MVP.Infrastructure.Persistence;

namespace MVP.Infrastructure.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IApplicationDbContext context,
    IMapper mapper) : IIdentityService
{
    public async Task<List<Usuario>> GetUsuariosActivosAsync()
    {
        var appUsers = await userManager.Users
            .Where(u => !u.Borrado)
            .Include(u => u.UserRoles)
            .Include(u => u.Tenant)
            .ToListAsync();
        return mapper.Map<List<Usuario>>(appUsers);
    }

    public async Task<(List<Usuario> Items, int TotalCount)> GetUsuariosActivosPagedAsync(int pageNumber, int pageSize)
    {
        var query = userManager.Users.Where(u => !u.Borrado);
        int totalCount = await query.CountAsync();

        var items = await query
            .Include(u => u.UserRoles)
            .Include(u => u.Tenant)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (mapper.Map<List<Usuario>>(items), totalCount);
    }

    public async Task<Usuario?> GetUsuarioActivoByUidAsync(Guid uid)
    {
        var appUser = await userManager.Users
            .Include(u => u.UserRoles)
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Uid == uid && !u.Borrado);
        
        return appUser == null ? null : mapper.Map<Usuario>(appUser);
    }

    public async Task<ApplicationResult> CrearUsuarioAsync(Usuario usuario, string password, List<string> rolesNombres)
    {
        var appUser = mapper.Map<ApplicationUser>(usuario);
        appUser.UserName = usuario.UserName ?? usuario.Email;
        appUser.Email = usuario.Email;

        await using var transaction = await context.BeginTransactionAsync();
        try
        {
            var result = await userManager.CreateAsync(appUser, password);

            if (result.Succeeded && rolesNombres.Any())
            {
                var roleResult = await userManager.AddToRolesAsync(appUser, rolesNombres);
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return ApplicationResult.Failure(roleResult.Errors.Select(e => e.Description));
                }
            }

            if (result.Succeeded)
            {
                await transaction.CommitAsync();
                return ApplicationResult.Success();
            }

            await transaction.RollbackAsync();
            return ApplicationResult.Failure(result.Errors.Select(e => e.Description));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApplicationResult> ActualizarUsuarioAsync(Usuario usuario, List<string> rolesNombres)
    {
        var appUser = await userManager.Users.FirstOrDefaultAsync(u => u.Uid == usuario.Uid);
        if (appUser == null)
            return ApplicationResult.Failure(new[] { "Usuario no encontrado." });

        mapper.Map(usuario, appUser);

        await using var transaction = await context.BeginTransactionAsync();
        try
        {
            var result = await userManager.UpdateAsync(appUser);

            if (result.Succeeded)
            {
                var rolesActuales = await userManager.GetRolesAsync(appUser);
                if (rolesActuales.Any())
                    await userManager.RemoveFromRolesAsync(appUser, rolesActuales);

                if (rolesNombres.Any())
                    await userManager.AddToRolesAsync(appUser, rolesNombres);

                await transaction.CommitAsync();
                return ApplicationResult.Success();
            }

            await transaction.RollbackAsync();
            return ApplicationResult.Failure(result.Errors.Select(e => e.Description));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApplicationResult> BorrarUsuarioAsync(Guid uid)
    {
        var appUser = await userManager.Users.FirstOrDefaultAsync(u => u.Uid == uid && !u.Borrado);
        if (appUser == null) 
            return ApplicationResult.Failure("Usuario no encontrado.", ErrorType.NotFound);

        appUser.Borrado = true;
        await userManager.UpdateAsync(appUser);
        return ApplicationResult.Success();
    }

    public async Task<List<Rol>> GetRolesByIdsAsync(IEnumerable<int> roleIds)
    {
        var roles = await roleManager.Roles
            .Include(r => r.Tenant)
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync();
        return mapper.Map<List<Rol>>(roles);
    }

    public async Task<List<Rol>> GetRolesByUidsAsync(IEnumerable<Guid> roleUids)
    {
        var roles = await roleManager.Roles
            .Include(r => r.Tenant)
            .Where(r => roleUids.Contains(r.Uid))
            .ToListAsync();
        return mapper.Map<List<Rol>>(roles);
    }

    public async Task<List<Rol>> GetRolesActivosAsync()
    {
        var roles = await roleManager.Roles
            .Include(r => r.Tenant)
            .Where(r => !r.Borrado)
            .ToListAsync();
        return mapper.Map<List<Rol>>(roles);
    }

    public async Task<ApplicationResult> CrearRolAsync(Rol rol)
    {
        var appRole = mapper.Map<ApplicationRole>(rol);
        var result = await roleManager.CreateAsync(appRole);
        return result.Succeeded
            ? ApplicationResult.Success()
            : ApplicationResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<bool> RolExisteAsync(string rolNombre)
    {
        return await roleManager.RoleExistsAsync(rolNombre);
    }
}

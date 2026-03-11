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

namespace MVP.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<Usuario>> GetUsuariosActivosAsync()
    {
        var appUsers = await _userManager.Users
            .Where(u => !u.Borrado)
            .Include(u => u.UserRoles)
            .ToListAsync();
        return _mapper.Map<List<Usuario>>(appUsers);
    }

    public async Task<(List<Usuario> Items, int TotalCount)> GetUsuariosActivosPagedAsync(int pageNumber, int pageSize)
    {
        var query = _userManager.Users.Where(u => !u.Borrado);
        int totalCount = await query.CountAsync();

        var items = await query
            .Include(u => u.UserRoles)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (_mapper.Map<List<Usuario>>(items), totalCount);
    }

    public async Task<Usuario?> GetUsuarioActivoByUidAsync(Guid uid)
    {
        var appUser = await _userManager.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Uid == uid && !u.Borrado);
        return _mapper.Map<Usuario>(appUser);
    }

    public async Task<ApplicationResult> CrearUsuarioAsync(Usuario usuario, string password, List<string> rolesNombres)
    {
        var appUser = _mapper.Map<ApplicationUser>(usuario);
        appUser.UserName = usuario.UserName ?? usuario.Email;
        appUser.Email = usuario.Email;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await _userManager.CreateAsync(appUser, password);

            if (result.Succeeded && rolesNombres.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(appUser, rolesNombres);
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
        var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Uid == usuario.Uid);
        if (appUser == null)
            return ApplicationResult.Failure(new[] { "Usuario no encontrado." });

        _mapper.Map(usuario, appUser);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await _userManager.UpdateAsync(appUser);

            if (result.Succeeded)
            {
                var rolesActuales = await _userManager.GetRolesAsync(appUser);
                if (rolesActuales.Any())
                    await _userManager.RemoveFromRolesAsync(appUser, rolesActuales);

                if (rolesNombres.Any())
                    await _userManager.AddToRolesAsync(appUser, rolesNombres);

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
        var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Uid == uid && !u.Borrado);
        if (appUser == null) return false;

        appUser.Borrado = true;
        await _userManager.UpdateAsync(appUser);
        return true;
    }

    public async Task<List<Rol>> GetRolesByIdsAsync(IEnumerable<int> roleIds)
    {
        var roles = await _roleManager.Roles
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync();
        return _mapper.Map<List<Rol>>(roles);
    }

    public async Task<List<Rol>> GetRolesByUidsAsync(IEnumerable<Guid> roleUids)
    {
        var roles = await _roleManager.Roles
            .Where(r => roleUids.Contains(r.Uid))
            .ToListAsync();
        return _mapper.Map<List<Rol>>(roles);
    }

    public async Task<List<Rol>> GetRolesActivosAsync()
    {
        var roles = await _roleManager.Roles
            .Where(r => !r.Borrado)
            .ToListAsync();
        return _mapper.Map<List<Rol>>(roles);
    }

    public async Task<ApplicationResult> CrearRolAsync(Rol rol)
    {
        var appRole = _mapper.Map<ApplicationRole>(rol);
        var result = await _roleManager.CreateAsync(appRole);
        return result.Succeeded
            ? ApplicationResult.Success()
            : ApplicationResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<bool> RolExisteAsync(string rolNombre)
    {
        return await _roleManager.RoleExistsAsync(rolNombre);
    }
}

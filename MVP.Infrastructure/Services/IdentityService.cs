using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVP.Application.Interfaces;
using MVP.Domain.Constants;
using MVP.Domain.Entities;
using MVP.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MVP.Application.Constants;

namespace MVP.Infrastructure.Services;

public class IdentityService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    ApplicationDbContext context,
    IUnitOfWork unitOfWork,
    Microsoft.Extensions.Caching.Hybrid.HybridCache cache,
    ILogger<IdentityService> logger) : IIdentityService
{
    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await userManager.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.Tenant)
            .ToListAsync();
    }

    public async Task<(List<User> Items, int TotalCount)> GetActiveUsersPagedAsync(int pageNumber, int pageSize)
    {
        var query = userManager.Users.Where(u => !u.IsDeleted);
        int totalCount = await query.CountAsync();

        var items = await query
            .Include(u => u.Tenant)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<User?> GetActiveUserByUidAsync(Guid uid)
    {
        return await userManager.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Uid == uid && !u.IsDeleted);
    }

    public async Task<ApplicationResult> CreateUserAsync(User user, string password, List<string> roleNames)
    {
        user.UserName ??= user.Email;

        var isInternalTransaction = context.Database.CurrentTransaction == null;
        if (isInternalTransaction) await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                if (roleNames.Any())
                {
                    var roleResult = await userManager.AddToRolesAsync(user, roleNames);
                    if (!roleResult.Succeeded)
                    {
                        if (isInternalTransaction) await unitOfWork.RollbackTransactionAsync();
                        return ApplicationResult.Failure(roleResult.Errors.Select(e => e.Description));
                    }
                }

                if (isInternalTransaction) await unitOfWork.CommitTransactionAsync();
                return ApplicationResult.Success();
            }

            if (isInternalTransaction) await unitOfWork.RollbackTransactionAsync();
            return ApplicationResult.Failure(result.Errors.Select(e => e.Description));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user {Email}", user.Email);
            if (isInternalTransaction) await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ApplicationResult> UpdateUserAsync(User user, List<string> roleNames)
    {
        var existingUser = await userManager.Users.FirstOrDefaultAsync(u => u.Uid == user.Uid);
        if (existingUser == null)
            return ApplicationResult.Failure(new[] { ErrorMessages.UserNotFound });

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.SecondLastName = user.SecondLastName;
        existingUser.FriendlyName = user.FriendlyName;
        existingUser.Email = user.Email;
        existingUser.UserName = user.Email;
        existingUser.PhoneNumber = user.PhoneNumber;
        existingUser.CatStatusAccountId = user.CatStatusAccountId;
        existingUser.TenantId = user.TenantId;

        var isInternalTransaction = context.Database.CurrentTransaction == null;
        if (isInternalTransaction) await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await userManager.UpdateAsync(existingUser);

            if (result.Succeeded)
            {
                var currentRoles = await userManager.GetRolesAsync(existingUser);
                if (currentRoles.Any())
                    await userManager.RemoveFromRolesAsync(existingUser, currentRoles);

                if (roleNames.Any())
                    await userManager.AddToRolesAsync(existingUser, roleNames);

                if (isInternalTransaction) await unitOfWork.CommitTransactionAsync();
                await cache.RemoveByTagAsync(CacheTags.UserMenu(existingUser.Uid));
                return ApplicationResult.Success();
            }

            if (isInternalTransaction) await unitOfWork.RollbackTransactionAsync();
            return ApplicationResult.Failure(result.Errors.Select(e => e.Description));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {Email}", user.Email);
            if (isInternalTransaction) await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ApplicationResult> DeleteUserAsync(Guid uid)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Uid == uid && !u.IsDeleted);
        if (user == null) 
            return ApplicationResult.Failure(ErrorMessages.UserNotFound, ErrorType.NotFound);

        user.IsDeleted = true;
        await userManager.UpdateAsync(user);
        return ApplicationResult.Success();
    }

    public async Task<List<string>> GetUserRolesNamesAsync(User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<List<Role>> GetRolesByIdsAsync(IEnumerable<int> roleIds)
    {
        return await roleManager.Roles
            .Include(r => r.Tenant)
            .Include(r => r.RoleModules)
                .ThenInclude(rm => rm.Module)
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync();
    }

    public async Task<List<Role>> GetRolesByUidsAsync(IEnumerable<Guid> roleUids)
    {
        return await roleManager.Roles
            .Include(r => r.Tenant)
            .Include(r => r.RoleModules)
                .ThenInclude(rm => rm.Module)
            .Where(r => roleUids.Contains(r.Uid))
            .ToListAsync();
    }

    public async Task<List<Role>> GetActiveRolesAsync()
    {
        return await roleManager.Roles
            .Include(r => r.Tenant)
            .Include(r => r.RoleModules)
                .ThenInclude(rm => rm.Module)
            .Where(r => !r.IsDeleted)
            .ToListAsync();
    }

    public async Task<ApplicationResult> CreateRoleAsync(Role role)
    {
        var result = await roleManager.CreateAsync(role);
        return result.Succeeded
            ? ApplicationResult.Success()
            : ApplicationResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await roleManager.RoleExistsAsync(roleName);
    }
}

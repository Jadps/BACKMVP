using AutoMapper;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Users;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MVP.Application.Services.Users;

public class UserService(
    IIdentityService identityService,
    ICurrentTenantService currentTenantService,
    IApplicationDbContext context,
    IMapper mapper) : IUserService
{
    public async Task<ApplicationResult<List<UserDto>>> GetAllActiveAsync()
    {
        var users = await identityService.GetActiveUsersAsync();
        var dtos = new List<UserDto>();

        foreach (var user in users)
        {
            var dto = mapper.Map<UserDto>(user);
            dto.FullName = user.FullName;
            await PopulateUserDtoRolesAsync(user, dto);
            dtos.Add(dto);
        }

        return ApplicationResult<List<UserDto>>.Success(dtos);
    }

    public async Task<ApplicationResult<PagedResult<UserDto>>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var result = await identityService.GetActiveUsersPagedAsync(pageNumber, pageSize);
        var dtos = new List<UserDto>();

        foreach (var user in result.Items)
        {
            var dto = mapper.Map<UserDto>(user);
            dto.FullName = user.FullName;
            await PopulateUserDtoRolesAsync(user, dto);
            dtos.Add(dto);
        }
        
        return ApplicationResult<PagedResult<UserDto>>.Success(new PagedResult<UserDto> 
        { 
            Items = dtos, 
            TotalCount = result.TotalCount 
        });
    }

    public async Task<ApplicationResult<UserDto>> GetByUidAsync(Guid uid)
    {
        if (uid == Guid.Empty) return ApplicationResult<UserDto>.Failure("Empty UID.", ErrorType.Validation);
        
        var user = await identityService.GetActiveUserByUidAsync(uid);
        if (user == null) return ApplicationResult<UserDto>.Failure("User not found.", ErrorType.NotFound);

        var dto = mapper.Map<UserDto>(user);
        dto.FullName = user.FullName;
        await PopulateUserDtoRolesAsync(user, dto);
        
        return ApplicationResult<UserDto>.Success(dto);
    }

    public async Task<ApplicationResult> CreateAsync(UserDto dto)
    {
        var user = new User
        {
            Uid = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            SecondLastName = dto.SecondLastName,
            FriendlyName = dto.FriendlyName,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CatStatusAccountId = dto.CatStatusAccountId,
            IsDeleted = false
        };
        
        if (dto.TenantId.HasValue)
        {
            var tenant = await context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Uid == dto.TenantId.Value);
            if (tenant == null) return ApplicationResult.Failure("Provided Tenant does not exist.", ErrorType.NotFound);
            user.TenantId = tenant.Id;
        }
        else if (!currentTenantService.IsSuperAdmin)
        {
            user.TenantId = currentTenantService.TenantId;
        }
        
        var newRoleNames = dto.Roles.Select(r => r.Name).Where(n => !string.IsNullOrEmpty(n)).Cast<string>().ToList();
        return await identityService.CreateUserAsync(user, dto.Password!, newRoleNames);
    }

    public async Task<ApplicationResult> UpdateAsync(UserDto dto)
    {
        if (dto.Id == null) return ApplicationResult.Failure("User identifier is null.", ErrorType.Validation);

        var updateInfo = new User
        {
            Uid = dto.Id.Value,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            SecondLastName = dto.SecondLastName,
            FriendlyName = dto.FriendlyName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CatStatusAccountId = dto.CatStatusAccountId
        };
        
        if (dto.TenantId.HasValue)
        {
            var tenant = await context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Uid == dto.TenantId.Value);
            if (tenant == null) return ApplicationResult.Failure("Provided Tenant does not exist.", ErrorType.NotFound);
            updateInfo.TenantId = tenant.Id;
        }

        var newRoleNames = dto.Roles.Select(r => r.Name).Where(n => !string.IsNullOrEmpty(n)).Cast<string>().ToList();
        return await identityService.UpdateUserAsync(updateInfo, newRoleNames);
    }

    public async Task<ApplicationResult> DeleteAsync(Guid id)
    {
        return await identityService.DeleteUserAsync(id);
    }

    private async Task PopulateUserDtoRolesAsync(User user, UserDto dto)
    {
        var roleNames = await identityService.GetUserRolesNamesAsync(user);
        var allRoles = await identityService.GetActiveRolesAsync();
        
        var userRoles = allRoles.Where(r => roleNames.Contains(r.Name ?? string.Empty)).ToList();
        dto.Roles = mapper.Map<List<RoleDto>>(userRoles);
    }
}

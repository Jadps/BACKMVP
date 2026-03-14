using MVP.Application.DTOs;
using MVP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces;

public interface IIdentityService
{
    Task<List<User>> GetActiveUsersAsync();
    Task<(List<User> Items, int TotalCount)> GetActiveUsersPagedAsync(int pageNumber, int pageSize);
    Task<User?> GetActiveUserByUidAsync(Guid uid);
    Task<ApplicationResult> CreateUserAsync(User user, string password, List<string> roleNames);
    Task<ApplicationResult> UpdateUserAsync(User user, List<string> roleNames);
    Task<ApplicationResult> DeleteUserAsync(Guid uid);
    Task<List<string>> GetUserRolesNamesAsync(User user);

    Task<List<Role>> GetRolesByIdsAsync(IEnumerable<int> roleIds);
    Task<List<Role>> GetRolesByUidsAsync(IEnumerable<Guid> roleUids);
    Task<List<Role>> GetActiveRolesAsync();
    Task<ApplicationResult> CreateRoleAsync(Role role);
    Task<bool> RoleExistsAsync(string roleName);
}

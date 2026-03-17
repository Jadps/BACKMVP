using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces.Catalogs;

public interface ICatalogService
{
    Task<ApplicationResult<List<RoleDto>>> GetRolesAsync();
    Task<ApplicationResult> CreateRoleAsync(RoleDto dto);
    Task<ApplicationResult<List<ModuleDto>>> GetMenuModulesAsync();
}

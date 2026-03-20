using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Catalogs;
using MVP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using MVP.WebAPI.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class CatalogsController(
    ICatalogService catalogService, 
    IGenericCatalogService genericService) : ControllerBase
{
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles() 
    {
        var result = await catalogService.GetRolesAsync();
        return result.ToActionResult();
    }

    [HttpPost("roles")]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> PostRole(RoleDto dto)
    {
        var result = await catalogService.CreateRoleAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut("roles")]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> PutRole(RoleDto dto)
    {
        var result = await catalogService.UpdateRoleAsync(dto);
        return result.ToActionResult();
    }

    [HttpGet("modules")]
    [Authorize]
    public async Task<IActionResult> GetModules()
    {
        var result = await catalogService.GetMenuModulesAsync();
        return result.ToActionResult();
    }

    [HttpGet("c/{name}")]
    [Authorize]
    public async Task<IActionResult> GetGeneric(string name)
    {
        var items = await genericService.GetCatalogAsync(name);
        return ApplicationResult<List<CatalogItemDto>>.Success(items).ToActionResult();
    }

    [HttpGet("tenants")]
    [Authorize]
    public async Task<IActionResult> GetTenants()
    {
        var items = await genericService.GetCatalogAsync("Tenants");
        return ApplicationResult<List<CatalogItemDto>>.Success(items).ToActionResult();
    }
}

using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Application.Interfaces.Catalogos;
using MVP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using MVP.WebAPI.Extensions;
using Microsoft.AspNetCore.OutputCaching;
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
    [OutputCache(Duration = 3600)]
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

    [HttpGet("modules")]
    [OutputCache(Duration = 3600)]
    public async Task<IActionResult> GetModules()
    {
        var result = await catalogService.GetMenuModulesAsync();
        return result.ToActionResult();
    }

    [HttpGet("c/{name}")]
    public async Task<IActionResult> GetGeneric(string name)
    {
        var items = await genericService.GetCatalogAsync(name);
        return ApplicationResult<List<CatalogItemDto>>.Success(items).ToActionResult();
    }

    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants()
    {
        var items = await genericService.GetCatalogAsync("Tenants");
        return ApplicationResult<List<CatalogItemDto>>.Success(items).ToActionResult();
    }
}

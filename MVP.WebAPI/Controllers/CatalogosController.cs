using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using MVP.Application.DTOs;
using MVP.Application.Interfaces.Catalogos;
using MVP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using MVP.WebAPI.Extensions;

using Microsoft.AspNetCore.OutputCaching;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class CatalogosController(
    ICatalogoService catalogoService, 
    IGenericCatalogService genericService) : ControllerBase
{
    [HttpGet("roles")]
    [OutputCache(Duration = 3600)]
    public async Task<IActionResult> GetRoles() => Ok(await catalogoService.GetRolesAsync());

    [HttpPost("roles")]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> PostRol(RolDTO dto)
    {
        var result = await catalogoService.CrearRolAsync(dto);
        return result.ToActionResult();
    }

    [HttpGet("modulos")]
    [OutputCache(Duration = 3600)]
    public async Task<IActionResult> GetModulos() => Ok(await catalogoService.GetModulosMenuAsync());

    [HttpGet("c/{nombre}")]
    public async Task<ActionResult<List<CatalogoItemDTO>>> GetGeneric(string nombre)
    {
        var items = await genericService.GetCatalogoAsync(nombre);
        return Ok(items);
    }

    [HttpGet("tenants")]
    public async Task<ActionResult<List<CatalogoItemDTO>>> GetTenants() => Ok(await genericService.GetCatalogoAsync("Tenants"));
}

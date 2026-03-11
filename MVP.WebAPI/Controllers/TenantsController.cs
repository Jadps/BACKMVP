using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using MVP.Application.DTOs;
using MVP.Application.Interfaces;
using MVP.Domain.Constants;

using MVP.WebAPI.Extensions;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class TenantsController(ITenantService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TenantDTO>>> Get() => Ok(await service.GetTodosAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> Post(TenantDTO dto)
    {
        var result = await service.CrearAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> Put(TenantDTO dto)
    {
        var result = await service.ActualizarAsync(dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await service.EliminarAsync(id);
        return result.ToActionResult();
    }
}

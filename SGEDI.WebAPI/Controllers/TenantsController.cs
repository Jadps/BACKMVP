using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces;
using SGEDI.Domain.Constants;

namespace SGEDI.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _service;

    public TenantsController(ITenantService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<TenantDTO>>> Get() => Ok(await _service.GetTodosAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<TenantDTO>> GetById(Guid id)
    {
        var tenant = await _service.GetByIdAsync(id);
        return tenant != null ? Ok(tenant) : NotFound();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> Post(TenantDTO dto)
    {
        var id = await _service.CrearAsync(dto);
        return Ok(new { Id = id });
    }

    [HttpPut]
    [Authorize(Roles = AppRoles.GlobalAdmin)]
    public async Task<IActionResult> Put(TenantDTO dto)
    {
        await _service.ActualizarAsync(dto);
        return Ok();
    }
}

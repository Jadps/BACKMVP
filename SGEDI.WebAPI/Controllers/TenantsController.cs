using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces;

namespace SGEDI.WebAPI.Controllers;

[Route("api/[controller]")]
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
    public async Task<ActionResult<TenantDTO>> GetById(string id)
    {
        var tenant = await _service.GetByIdAsync(id);
        return tenant != null ? Ok(tenant) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Post(TenantDTO dto)
    {
        var id = await _service.CrearAsync(dto);
        return Ok(new { Id = id });
    }

    [HttpPut]
    public async Task<IActionResult> Put(TenantDTO dto)
    {
        await _service.ActualizarAsync(dto);
        return Ok();
    }
}

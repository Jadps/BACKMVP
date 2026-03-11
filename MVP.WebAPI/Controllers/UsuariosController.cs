using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using MVP.Application.DTOs;
using MVP.Application.Interfaces.Usuarios;
using MVP.Domain.Constants;

using MVP.WebAPI.Extensions;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsuariosController(IUsuarioService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10) 
        => Ok(await service.GetPagedAsync(pageNumber, pageSize));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await service.GetPerfilActualAsync();
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.GlobalAdmin + "," + AppRoles.TenantAdmin)]
    public async Task<IActionResult> Post(UsuarioDTO dto)
    {
        var result = await service.CrearAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut]
    [Authorize(Roles = AppRoles.GlobalAdmin + "," + AppRoles.TenantAdmin)]
    public async Task<IActionResult> Put(UsuarioDTO dto)
    {
        var result = await service.ActualizarAsync(dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.GlobalAdmin + "," + AppRoles.TenantAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await service.BorrarAsync(id);
        return result.ToActionResult();
    }
}

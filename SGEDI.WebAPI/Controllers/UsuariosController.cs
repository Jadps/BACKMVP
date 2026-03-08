using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces.Usuarios;
using SGEDI.Domain.Constants;

namespace SGEDI.WebAPI.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _service;

        public UsuariosController(IUsuarioService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _service.GetTodosAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _service.GetByIdAsync(id);
            return user != null ? Ok(user) : NotFound();
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.GlobalAdmin + "," + AppRoles.TenantAdmin)]
        public async Task<IActionResult> Post(UsuarioDTO dto)
        {
            var result = await _service.CrearAsync(dto);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [HttpPut]
        [Authorize(Roles = AppRoles.GlobalAdmin + "," + AppRoles.TenantAdmin)]
        public async Task<IActionResult> Put(UsuarioDTO dto)
        {
            var result = await _service.ActualizarAsync(dto);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.GlobalAdmin + "," + AppRoles.TenantAdmin)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var borrado = await _service.BorrarAsync(id);
            return borrado ? Ok() : NotFound();
        }
    }
}

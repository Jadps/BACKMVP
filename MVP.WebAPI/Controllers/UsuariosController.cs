using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using MVP.Application.DTOs;
using MVP.Application.Interfaces.Usuarios;
using MVP.Domain.Constants;

namespace MVP.WebAPI.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _service;

        public UsuariosController(IUsuarioService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10) 
            => Ok(await _service.GetPagedAsync(pageNumber, pageSize));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _service.GetByIdAsync(id);
            return user != null ? Ok(user) : NotFound();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var user = await _service.GetPerfilActualAsync();
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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces.Usuarios;

namespace SGEDI.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> Post(UsuarioDTO dto)
        {
            var result = await _service.CrearAsync(dto);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [HttpPut]
        public async Task<IActionResult> Put(UsuarioDTO dto)
        {
            var result = await _service.ActualizarAsync(dto);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var borrado = await _service.BorrarAsync(id);
            return borrado ? Ok() : NotFound();
        }
    }
}

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

        [HttpPost]
        public async Task<IActionResult> Post(UsuarioDTO dto)
        {
            var result = await _service.CrearAsync(dto);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }
    }
}

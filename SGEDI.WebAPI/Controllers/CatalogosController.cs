using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SGEDI.Application.DTOs;
using SGEDI.Application.Interfaces.Catalogos;

namespace SGEDI.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogosController : ControllerBase
    {
        private readonly ICatalogoService _service;

        public CatalogosController(ICatalogoService service) => _service = service;

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles() => Ok(await _service.GetRolesAsync());

        [HttpPost("roles")]
        public async Task<IActionResult> PostRol(RolDTO dto)
        {
            await _service.CrearRolAsync(dto);
            return Ok();
        }

        [HttpGet("modulos")]
        public async Task<IActionResult> GetModulos() => Ok(await _service.GetModulosMenuAsync());
    }
}


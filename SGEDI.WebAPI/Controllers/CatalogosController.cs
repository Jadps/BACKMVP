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
        private readonly IGenericCatalogService _genericService;

        public CatalogosController(ICatalogoService service, IGenericCatalogService genericService)
        {
            _service = service;
            _genericService = genericService;
        }

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

        [HttpGet("{nombre}")]
        public async Task<ActionResult<List<CatalogoItemDTO>>> GetGeneric(string nombre)
        {
            var items = await _genericService.GetCatalogoAsync(nombre);
            return Ok(items);
        }

        [HttpGet("tenants")]
        public async Task<ActionResult<List<CatalogoItemDTO>>> GetTenants() => Ok(await _genericService.GetCatalogoAsync("Tenants"));
    }
}


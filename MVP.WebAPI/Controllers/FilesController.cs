using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVP.Application.Interfaces;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IArchivoService _archivoService;

    public FilesController(IArchivoService archivoService)
    {
        _archivoService = archivoService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile requestFile, [FromForm] string entidadTipo, [FromForm] string entidadId)
    {
        if (requestFile == null || requestFile.Length == 0)
            return BadRequest(new { message = "El archivo está vacío o no fue enviado." });

        if (string.IsNullOrWhiteSpace(entidadTipo) || string.IsNullOrWhiteSpace(entidadId))
            return BadRequest(new { message = "EntidadTipo y EntidadId son obligatorios." });

        using var stream = requestFile.OpenReadStream();
        var result = await _archivoService.UploadArchivoAsync(
            stream, 
            requestFile.FileName, 
            requestFile.ContentType, 
            entidadTipo, 
            entidadId);

        if (result.Succeeded)
            return Ok(new { result.Value!.Uid, result.Value.NombreOriginal });

        return BadRequest(new { result.Errors });
    }

    [HttpGet("{uid}")]
    public async Task<IActionResult> Download(Guid uid)
    {
        var result = await _archivoService.DownloadArchivoAsync(uid);
        if (!result.Succeeded)
            return NotFound(new { result.Errors });

        // Assuming you might want to attach original filename, but here we just stream it
        return File(result.Value!, "application/octet-stream");
    }

    [HttpDelete("{uid}")]
    public async Task<IActionResult> Delete(Guid uid)
    {
        var result = await _archivoService.SoftDeleteArchivoAsync(uid);
        if (result.Succeeded)
            return Ok(new { message = "Archivo eliminado lógicamente." });

        return NotFound(new { result.Errors });
    }
}

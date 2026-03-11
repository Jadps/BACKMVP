using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVP.Application.Interfaces;

using MVP.WebAPI.Extensions;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class FilesController(IArchivoService archivoService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile requestFile, [FromForm] string entidadTipo, [FromForm] string entidadId)
    {
        if (requestFile == null || requestFile.Length == 0)
            return BadRequest(new { message = "El archivo está vacío o no fue enviado." });

        if (string.IsNullOrWhiteSpace(entidadTipo) || string.IsNullOrWhiteSpace(entidadId))
            return BadRequest(new { message = "EntidadTipo y EntidadId son obligatorios." });

        using var stream = requestFile.OpenReadStream();
        var result = await archivoService.UploadArchivoAsync(
            stream, 
            requestFile.FileName, 
            requestFile.ContentType, 
            entidadTipo, 
            entidadId);

        return result.ToActionResult();
    }

    [HttpGet("{uid}")]
    public async Task<IActionResult> Download(Guid uid)
    {
        var result = await archivoService.DownloadArchivoAsync(uid);
        if (!result.IsSuccess)
            return result.ToActionResult();

        return File(result.Data!, "application/octet-stream");
    }

    [HttpDelete("{uid}")]
    public async Task<IActionResult> Delete(Guid uid)
    {
        var result = await archivoService.SoftDeleteArchivoAsync(uid);
        return result.ToActionResult();
    }
}

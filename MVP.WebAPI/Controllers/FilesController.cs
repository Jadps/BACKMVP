using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using MVP.Application.Interfaces;
using MVP.WebAPI.Extensions;
using System;
using System.Threading.Tasks;

namespace MVP.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class FilesController(IFileService fileService) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string entityType, [FromQuery] string entityId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var result = await fileService.UploadFileAsync(stream, file.FileName, file.ContentType, entityType, entityId);
        
        return result.ToActionResult();
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var result = await fileService.DownloadFileAsync(id);
        if (!result.IsSuccess) return result.ToActionResult();

        return File(result.Data!, "application/octet-stream");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await fileService.SoftDeleteFileAsync(id);
        return result.ToActionResult();
    }
}

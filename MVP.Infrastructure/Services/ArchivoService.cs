using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MVP.Application.Interfaces;
using MVP.Domain.Entities;
using MVP.Infrastructure.Persistence;

namespace MVP.Infrastructure.Services;

public class ArchivoService(
    ApplicationDbContext dbContext, 
    IFileStorageService fileStorageService,
    ICurrentTenantService currentTenantService) : IArchivoService
{
    public async Task<ApplicationResult<Archivo>> UploadArchivoAsync(Stream fileStream, string fileName, string contentType, string entidadTipo, string entidadId)
    {
        if (!currentTenantService.TenantId.HasValue)
        {
            return ApplicationResult<Archivo>.Failure(new[] { "No se puede subir un archivo sin un TenantId activo." }, ErrorType.Validation);
        }

        var sftpResult = await fileStorageService.UploadFileAsync(fileStream, fileName, contentType);
        if (!sftpResult.IsSuccess)
        {
            return ApplicationResult<Archivo>.Failure(sftpResult.Errors, sftpResult.ErrorType);
        }

        var extension = Path.GetExtension(fileName);
        var archivo = new Archivo
        {
            NombreOriginal = fileName,
            Extension = extension,
            ContentType = contentType,
            TamanoBytes = fileStream.Length,
            RutaFisica = sftpResult.Data!,
            EntidadTipo = entidadTipo,
            EntidadId = entidadId,
            TenantId = currentTenantService.TenantId.Value
        };

        dbContext.Archivos.Add(archivo);
        await dbContext.SaveChangesAsync();

        return ApplicationResult<Archivo>.Success(archivo);
    }

    public async Task<ApplicationResult<Stream>> DownloadArchivoAsync(Guid archivoUid)
    {
        var archivo = await dbContext.Archivos.FirstOrDefaultAsync(a => a.Uid == archivoUid);
        if (archivo == null)
        {
            return ApplicationResult<Stream>.Failure(new[] { "Archivo no encontrado o acceso denegado." }, ErrorType.NotFound);
        }

        return await fileStorageService.DownloadFileAsync(archivo.RutaFisica);
    }

    public async Task<ApplicationResult> SoftDeleteArchivoAsync(Guid archivoUid)
    {
        var archivo = await dbContext.Archivos.FirstOrDefaultAsync(a => a.Uid == archivoUid);
        if (archivo == null)
        {
            return ApplicationResult.Failure(new[] { "Archivo no encontrado o acceso denegado." }, ErrorType.NotFound);
        }

        archivo.Borrado = true;
        await dbContext.SaveChangesAsync();

        return ApplicationResult.Success();
    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MVP.Application.Interfaces;
using MVP.Domain.Entities;
using MVP.Infrastructure.Persistence;

namespace MVP.Infrastructure.Services;

public class ArchivoService : IArchivoService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentTenantService _currentTenantService;

    public ArchivoService(
        ApplicationDbContext dbContext, 
        IFileStorageService fileStorageService,
        ICurrentTenantService currentTenantService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
        _currentTenantService = currentTenantService;
    }

    public async Task<ApplicationResult<Archivo>> UploadArchivoAsync(Stream fileStream, string fileName, string contentType, string entidadTipo, string entidadId)
    {
        if (!_currentTenantService.TenantId.HasValue)
        {
            return ApplicationResult<Archivo>.Failure(new[] { "No se puede subir un archivo sin un TenantId activo." }, ErrorType.Validation);
        }

        // 1. Upload to SFTP
        var sftpResult = await _fileStorageService.UploadFileAsync(fileStream, fileName, contentType);
        if (!sftpResult.Succeeded)
        {
            return ApplicationResult<Archivo>.Failure(sftpResult.Errors, sftpResult.ErrorType);
        }

        // 2. Register in Database
        var extension = Path.GetExtension(fileName);
        var archivo = new Archivo
        {
            NombreOriginal = fileName,
            Extension = extension,
            ContentType = contentType,
            TamanoBytes = fileStream.Length,
            RutaFisica = sftpResult.Value!,
            EntidadTipo = entidadTipo,
            EntidadId = entidadId,
            TenantId = _currentTenantService.TenantId.Value
        };

        _dbContext.Archivos.Add(archivo);
        await _dbContext.SaveChangesAsync();

        return ApplicationResult<Archivo>.Success(archivo);
    }

    public async Task<ApplicationResult<Stream>> DownloadArchivoAsync(Guid archivoUid)
    {
        var archivo = await _dbContext.Archivos.FirstOrDefaultAsync(a => a.Uid == archivoUid);
        if (archivo == null)
        {
            return ApplicationResult<Stream>.Failure(new[] { "Archivo no encontrado o acceso denegado." }, ErrorType.NotFound);
        }

        return await _fileStorageService.DownloadFileAsync(archivo.RutaFisica);
    }

    public async Task<ApplicationResult> SoftDeleteArchivoAsync(Guid archivoUid)
    {
        var archivo = await _dbContext.Archivos.FirstOrDefaultAsync(a => a.Uid == archivoUid);
        if (archivo == null)
        {
            return ApplicationResult.Failure(new[] { "Archivo no encontrado o acceso denegado." }, ErrorType.NotFound);
        }

        // Logical delete only. We intentionally DO NOT delete from SFTP to preserve history.
        archivo.Borrado = true;
        await _dbContext.SaveChangesAsync();

        return ApplicationResult.Success();
    }
}

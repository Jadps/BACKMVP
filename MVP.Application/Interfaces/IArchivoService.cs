using System;
using System.IO;
using System.Threading.Tasks;
using MVP.Domain.Entities;

namespace MVP.Application.Interfaces;

public interface IArchivoService
{
    Task<ApplicationResult<Archivo>> UploadArchivoAsync(Stream fileStream, string fileName, string contentType, string entidadTipo, string entidadId);
    Task<ApplicationResult<Stream>> DownloadArchivoAsync(Guid archivoUid);
    Task<ApplicationResult> SoftDeleteArchivoAsync(Guid archivoUid);
}

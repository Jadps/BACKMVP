using System;
using System.IO;
using System.Threading.Tasks;
using MVP.Domain.Entities;

namespace MVP.Application.Interfaces;

public interface IFileService
{
    Task<ApplicationResult<FileEntity>> UploadFileAsync(Stream fileStream, string fileName, string contentType, string entityType, string entityId);
    Task<ApplicationResult<Stream>> DownloadFileAsync(Guid fileUid);
    Task<ApplicationResult> SoftDeleteFileAsync(Guid fileUid);
}

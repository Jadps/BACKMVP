using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using MVP.Application.Interfaces;
using MVP.Domain.Entities;

namespace MVP.Infrastructure.Services;

public class FileService(
    IFileStorageService storageService,
    ICurrentTenantService currentTenantService) : IFileService
{
    public async Task<ApplicationResult<FileEntity>> UploadFileAsync(Stream fileStream, string fileName, string contentType, string entityType, string entityId)
    {
        var extension = Path.GetExtension(fileName);
        var storageResult = await storageService.SaveFileAsync(fileStream, fileName);
        
        if (!storageResult.IsSuccess) 
            return ApplicationResult<FileEntity>.Failure(storageResult.ErrorMessage!);

        var file = new FileEntity
        {
            Uid = Guid.NewGuid(),
            OriginalName = fileName,
            Extension = extension,
            ContentType = contentType,
            SizeBytes = fileStream.Length,
            PhysicalPath = storageResult.Data!,
            EntityType = entityType,
            EntityId = entityId,
            TenantId = currentTenantService.TenantId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        return ApplicationResult<FileEntity>.Success(file);
    }

    public async Task<ApplicationResult<Stream>> DownloadFileAsync(Guid fileUid)
    {
        return await Task.FromResult(ApplicationResult<Stream>.Failure("Not implemented", ErrorType.Validation));
    }

    public async Task<ApplicationResult> SoftDeleteFileAsync(Guid fileUid)
    {
        return await Task.FromResult(ApplicationResult.Success());
    }
}

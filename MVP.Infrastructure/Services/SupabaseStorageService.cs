using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVP.Application.Interfaces;

namespace MVP.Infrastructure.Services;

public interface ISupabaseStorageService : IFileStorageService
{
    Task<string> UploadFileAsync(byte[] fileBytes, string folderPath, string fileName, string contentType);
}

public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly ILogger<SupabaseStorageService> _logger;
    private readonly string _bucketName = "documents";
    private readonly string _defaultUploadPath = "uploads";

    public SupabaseStorageService(Supabase.Client supabaseClient, ILogger<SupabaseStorageService> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(byte[] fileBytes, string folderPath, string fileName, string contentType)
    {
        try 
        {
            var storage = _supabaseClient.Storage.From(_bucketName);
            var fullPath = $"{folderPath.TrimEnd('/')}/{fileName}";
            
            await storage.Upload(fileBytes, fullPath, new Supabase.Storage.FileOptions { ContentType = contentType, Upsert = true });

            var signedUrl = await storage.CreateSignedUrl(fullPath, 3600);
            
            return signedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Supabase Storage Error: Failed to upload or sign file {FileName} in {FolderPath}", fileName, folderPath);
            throw;
        }
    }

    public async Task<ApplicationResult<string>> SaveFileAsync(Stream fileStream, string fileName)
    {
        try
        {
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            var fullPath = $"{_defaultUploadPath}/{Guid.NewGuid()}_{fileName}";
            
            var storage = _supabaseClient.Storage.From(_bucketName);
            await storage.Upload(fileBytes, fullPath, new Supabase.Storage.FileOptions { Upsert = true });

            return ApplicationResult<string>.Success(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Supabase SaveFileAsync Error for file {FileName}", fileName);
            return ApplicationResult<string>.Failure($"Supabase Upload Error: {ex.Message}");
        }
    }

    public async Task<ApplicationResult<Stream>> GetFileAsync(string physicalPath)
    {
        try
        {
            var storage = _supabaseClient.Storage.From(_bucketName);
            var bytes = await storage.Download(physicalPath, null);
            return ApplicationResult<Stream>.Success(new MemoryStream(bytes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Supabase GetFileAsync Error for path {PhysicalPath}", physicalPath);
            return ApplicationResult<Stream>.Failure($"Supabase Download Error: {ex.Message}");
        }
    }

    public async Task<ApplicationResult> DeleteFileAsync(string physicalPath)
    {
        try
        {
            var storage = _supabaseClient.Storage.From(_bucketName);
            await storage.Remove(physicalPath);
            return ApplicationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Supabase DeleteFileAsync Error for path {PhysicalPath}", physicalPath);
            return ApplicationResult.Failure($"Supabase Delete Error: {ex.Message}");
        }
    }
}

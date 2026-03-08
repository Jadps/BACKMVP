using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MVP.Application.Interfaces;
using MVP.Infrastructure.Configuration;
using Renci.SshNet;

namespace MVP.Infrastructure.Services;

public class SftpStorageService : IFileStorageService
{
    private readonly SftpStorageOptions _options;

    public SftpStorageService(IOptions<SftpStorageOptions> options)
    {
        _options = options.Value;
    }

    private SftpClient CreateClient()
    {
        return new SftpClient(_options.Host, _options.Port, _options.Username, _options.Password);
    }

    public async Task<ApplicationResult<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try 
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var fullPath = Path.Combine(_options.BasePath, uniqueFileName).Replace('\\', '/');

            using var client = CreateClient();
            client.Connect();

            if (!client.Exists(_options.BasePath))
            {
                client.CreateDirectory(_options.BasePath);
            }
            await Task.Run(() => client.UploadFile(fileStream, fullPath));

            client.Disconnect();
            return ApplicationResult<string>.Success(fullPath);
        }
        catch (Exception ex)
        {
            return ApplicationResult<string>.Failure(new[] { $"Error subiendo el archivo: {ex.Message}" }, ErrorType.Unexpected);
        }
    }

    public async Task<ApplicationResult<Stream>> DownloadFileAsync(string fileUrlOrPath)
    {
        try 
        {
            using var client = CreateClient();
            client.Connect();

            if (!client.Exists(fileUrlOrPath))
            {
                return ApplicationResult<Stream>.Failure(new[] { $"El archivo en la ruta {fileUrlOrPath} no se encontró en el servidor SFTP." }, ErrorType.NotFound);
            }

            var memoryStream = new MemoryStream();
            await Task.Run(() => client.DownloadFile(fileUrlOrPath, memoryStream));
            client.Disconnect();

            memoryStream.Position = 0;
            return ApplicationResult<Stream>.Success(memoryStream);
        }
        catch (Exception ex)
        {
            return ApplicationResult<Stream>.Failure(new[] { $"Error descargando el archivo: {ex.Message}" }, ErrorType.Unexpected);
        }
    }

    public async Task<ApplicationResult> DeleteFileAsync(string fileUrlOrPath)
    {
        try 
        {
            using var client = CreateClient();
            client.Connect();

            if (client.Exists(fileUrlOrPath))
            {
                await Task.Run(() => client.DeleteFile(fileUrlOrPath));
            }

            client.Disconnect();
            return ApplicationResult.Success();
        }
        catch (Exception ex)
        {
            return ApplicationResult.Failure(new[] { $"Error borrando el archivo: {ex.Message}" }, ErrorType.Unexpected);
        }
    }
}

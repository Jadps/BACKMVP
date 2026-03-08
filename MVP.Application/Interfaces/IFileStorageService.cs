using System.IO;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces;

public interface IFileStorageService
{
    Task<ApplicationResult<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<ApplicationResult<Stream>> DownloadFileAsync(string fileUrlOrPath);
    Task<ApplicationResult> DeleteFileAsync(string fileUrlOrPath);
}

using System.IO;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces;

public interface IFileStorageService
{
    Task<ApplicationResult<string>> SaveFileAsync(Stream fileStream, string fileName);
    Task<ApplicationResult<Stream>> GetFileAsync(string physicalPath);
    Task<ApplicationResult> DeleteFileAsync(string physicalPath);
}

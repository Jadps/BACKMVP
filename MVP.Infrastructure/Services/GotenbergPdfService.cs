using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MVP.Infrastructure.Services;

public interface IPdfGeneratorService
{
    Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent);
}

public class GotenbergPdfService : IPdfGeneratorService
{
    private readonly HttpClient _httpClient;

    public GotenbergPdfService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent)
    {
        using var request = new MultipartFormDataContent();
        
        var htmlBytes = Encoding.UTF8.GetBytes(htmlContent);
        var htmlContentItem = new ByteArrayContent(htmlBytes);
        request.Add(htmlContentItem, "files", "index.html");

        var response = await _httpClient.PostAsync("forms/chromium/convert/html", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }
}

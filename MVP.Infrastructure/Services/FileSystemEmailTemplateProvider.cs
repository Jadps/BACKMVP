using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using MVP.Application.Interfaces;

namespace MVP.Infrastructure.Services;

public class FileSystemEmailTemplateProvider(IWebHostEnvironment env) : IEmailTemplateProvider
{
    private readonly string _templatesPath = Path.Combine(env.ContentRootPath, "EmailTemplates");

    public async Task<string> GetTemplateAsync(string templateName, IDictionary<string, string>? placeholders = null)
    {
        var filePath = Path.Combine(_templatesPath, $"{templateName}.html");
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Email template {templateName} not found at {filePath}");
        }

        var content = await File.ReadAllTextAsync(filePath);

        if (placeholders != null)
        {
            foreach (var placeholder in placeholders)
            {
                content = content.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }
        }

        return content;
    }
}

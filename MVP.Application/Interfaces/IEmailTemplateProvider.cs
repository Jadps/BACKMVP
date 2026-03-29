using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVP.Application.Interfaces;

public interface IEmailTemplateProvider
{
    Task<string> GetTemplateAsync(string templateName, IDictionary<string, string>? placeholders = null);
}

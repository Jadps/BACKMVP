using System.Threading.Tasks;

namespace MVP.Application.Interfaces;

public interface IEmailService
{
    Task<ApplicationResult> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
}

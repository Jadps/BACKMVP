using System.Threading.Tasks;

namespace MVP.Application.Interfaces;

public interface IEmailService
{
    Task<ApplicationResult> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task<ApplicationResult> SendWelcomeEmailAsync(string to, string userName);
    Task<ApplicationResult> SendPasswordResetEmailAsync(string to, string resetLink);
}

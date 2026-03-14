using System.Threading.Tasks;

namespace MVP.Application.Interfaces;

public interface IEmailService
{
    Task<ApplicationResult> SendEmailAsync(string to, string subject, string body);
    Task<ApplicationResult> SendWelcomeEmailAsync(string email, string name);
    Task<ApplicationResult> SendPasswordResetEmailAsync(string email, string resetLink);
}

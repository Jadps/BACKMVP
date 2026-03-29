using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using MVP.Application.Interfaces;
using MVP.Infrastructure.Configuration;

namespace MVP.Infrastructure.Services;

public class SmtpEmailService(
    IOptions<SmtpEmailOptions> options, 
    IEmailTemplateProvider templateProvider,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpEmailOptions _options = options.Value;

    public async Task<ApplicationResult> SendEmailAsync(string to, string subject, string body)
    {
        return await SendEmailInternalAsync(to, subject, body, true);
    }

    private async Task<ApplicationResult> SendEmailInternalAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            email.Body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Plain)
            {
                Text = body
            };

            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

            var secureSocketOptions = _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await smtp.ConnectAsync(_options.Server, _options.Port, secureSocketOptions);
            
            if (!string.IsNullOrEmpty(_options.Username))
            {
                await smtp.AuthenticateAsync(_options.Username, _options.Password);
            }

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return ApplicationResult.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocurrió un error enviando correo a {ToEmail}", to);
            return ApplicationResult.Failure(new[] { $"Error enviando correo: {ex.Message}" }, ErrorType.Unexpected);
        }
    }

    public async Task<ApplicationResult> SendWelcomeEmailAsync(string to, string userName)
    {
        var placeholders = new Dictionary<string, string> { { "UserName", userName } };
        var body = await templateProvider.GetTemplateAsync("welcome", placeholders);
        return await SendEmailInternalAsync(to, "Welcome to MVP Platform", body, true);
    }

    public async Task<ApplicationResult> SendPasswordResetEmailAsync(string to, string resetLink)
    {
        var placeholders = new Dictionary<string, string> { { "ResetLink", resetLink } };
        var body = await templateProvider.GetTemplateAsync("password_reset", placeholders);
        return await SendEmailInternalAsync(to, "Password Recovery - MVP", body, true);
    }
}

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

public class SmtpEmailService : IEmailService
{
    private readonly SmtpEmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpEmailOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ApplicationResult> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
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
            _logger.LogError(ex, "Ocurrió un error enviando correo a {ToEmail}", to);
            return ApplicationResult.Failure(new[] { $"Error enviando correo: {ex.Message}" }, ErrorType.Unexpected);
        }
    }
}

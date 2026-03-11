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

    public async Task<ApplicationResult> SendWelcomeEmailAsync(string to, string userName)
    {
        var subject = "Bienvenido a SGEDI - Registro Exitoso";
        var body = $@"
            <div style='font-family: sans-serif; color: #333;'>
                <h2>¡Hola {userName}!</h2>
                <p>Tu organización ha sido registrada correctamente en la plataforma <strong>SGEDI</strong>.</p>
                <p>Ya puedes acceder con tus credenciales de administrador.</p>
                <br/>
                <p>Saludos,<br/>El equipo de SGEDI</p>
            </div>";

        return await SendEmailAsync(to, subject, body, true);
    }

    public async Task<ApplicationResult> SendPasswordResetEmailAsync(string to, string resetLink)
    {
        var subject = "Recuperación de Contraseña - SGEDI";
        var body = $@"
            <div style='font-family: sans-serif; color: #333;'>
                <h3>Recuperación de Contraseña</h3>
                <p>Hemos recibido una solicitud para cambiar tu contraseña.</p>
                <p>Haz clic en el siguiente enlace para establecer una nueva contraseña:</p>
                <p><a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Recuperar mi contraseña</a></p>
                <br/>
                <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>
            </div>";

        return await SendEmailAsync(to, subject, body, true);
    }
}

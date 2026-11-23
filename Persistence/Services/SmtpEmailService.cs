using System.Net;
using System.Net.Mail;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Persistence.Services;

public class SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task EnviarEmailAsync(string destinatario, string assunto, string corpo)
    {
        var host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? configuration["Smtp:Host"];
        var portString = Environment.GetEnvironmentVariable("SMTP_PORT") ?? configuration["Smtp:Port"];
        var user = Environment.GetEnvironmentVariable("SMTP_USER") ?? configuration["Smtp:User"];
        var pass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? configuration["Smtp:Pass"];
        var from = Environment.GetEnvironmentVariable("SMTP_FROM") ?? configuration["Smtp:From"];

        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogWarning(
                "SMTP_HOST não configurado. [Simulação de E-mail] Destinatário: {Destinatario} | Assunto: '{Assunto}' | Corpo: '{Corpo}'",
                destinatario,
                assunto,
                corpo
            );
            return;
        }

        int port = int.TryParse(portString, out var p) ? p : 587;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = port != 25,
            Timeout = 10000
        };

        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
        {
            client.Credentials = new NetworkCredential(user, pass);
        }

        using var message = new MailMessage
        {
            From = new MailAddress(from, "Checkpoint"),
            Subject = assunto,
            Body = corpo,
            IsBodyHtml = false
        };
        message.To.Add(destinatario);

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar e-mail para {Destinatario}", destinatario);
            throw;
        }
    }
}

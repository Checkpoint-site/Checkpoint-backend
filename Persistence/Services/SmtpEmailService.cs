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
        var host = Environment.GetEnvironmentVariable("SMTP_HOST")
            ?? configuration["Smtp:Host"]
            ?? throw new InvalidOperationException("O host do SMTP (SMTP_HOST) não foi configurado.");

        var portString = Environment.GetEnvironmentVariable("SMTP_PORT")
            ?? configuration["Smtp:Port"]
            ?? throw new InvalidOperationException("A porta do SMTP (SMTP_PORT) não foi configurada.");

        if (!int.TryParse(portString, out var port))
        {
            throw new InvalidOperationException("A porta do SMTP (SMTP_PORT) deve ser um número válido.");
        }

        var user = Environment.GetEnvironmentVariable("SMTP_USER")
            ?? configuration["Smtp:User"]
            ?? throw new InvalidOperationException("O usuário do SMTP (SMTP_USER) não foi configurado.");

        var pass = Environment.GetEnvironmentVariable("SMTP_PASS")
            ?? configuration["Smtp:Pass"]
            ?? throw new InvalidOperationException("A senha do SMTP (SMTP_PASS) não foi configurada.");

        var from = Environment.GetEnvironmentVariable("SMTP_FROM")
            ?? configuration["Smtp:From"]
            ?? throw new InvalidOperationException("O e-mail de envio do SMTP (SMTP_FROM) não foi configurado.");

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = port != 25,
            Timeout = 10000,
            Credentials = new NetworkCredential(user, pass)
        };

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

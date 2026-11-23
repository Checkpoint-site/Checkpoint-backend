namespace Domain.Interfaces.Services;

public interface IEmailService
{
    Task EnviarEmailAsync(string destinatario, string assunto, string corpo);
}

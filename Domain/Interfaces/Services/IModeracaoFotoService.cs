namespace Domain.Interfaces.Services;

public interface IModeracaoFotoService
{
    Task<bool> FotoEhSeguraAsync(string? urlOuBase64);
}

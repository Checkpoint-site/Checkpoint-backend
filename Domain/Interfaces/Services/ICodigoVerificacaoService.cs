namespace Domain.Interfaces.Services;

public interface ICodigoVerificacaoService
{
    Task SalvarCodigoAsync(string email, string codigo, TimeSpan expiracao);
    Task<string?> ObterCodigoAsync(string email);
    Task RemoverCodigoAsync(string email);
}

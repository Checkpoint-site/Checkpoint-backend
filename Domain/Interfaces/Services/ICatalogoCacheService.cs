namespace Domain.Interfaces.Services;

public interface ICatalogoCacheService
{
    Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken = default);
    Task SalvarAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken cancellationToken = default);
    Task RemoverAsync(string chave, CancellationToken cancellationToken = default);
}

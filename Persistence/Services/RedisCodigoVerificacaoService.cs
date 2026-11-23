using Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Persistence.Services;

public class RedisCodigoVerificacaoService(IDistributedCache cache) : ICodigoVerificacaoService
{
    private static string ObterChave(string email) => $"codigo_verificacao:{email.Trim().ToLowerInvariant()}";

    public async Task SalvarCodigoAsync(string email, string codigo, TimeSpan expiracao)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiracao
        };

        await cache.SetStringAsync(ObterChave(email), codigo, options);
    }

    public async Task<string?> ObterCodigoAsync(string email)
    {
        return await cache.GetStringAsync(ObterChave(email));
    }

    public async Task RemoverCodigoAsync(string email)
    {
        await cache.RemoveAsync(ObterChave(email));
    }
}

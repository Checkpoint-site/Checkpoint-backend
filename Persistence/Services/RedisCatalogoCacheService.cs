using System.Text.Json;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Persistence.Services;

public class RedisCatalogoCacheService(
    IDistributedCache cache,
    ILogger<RedisCatalogoCacheService> logger
) : ICatalogoCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await cache.GetStringAsync(chave, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao ler chave {Chave} do cache Redis de catálogo.", chave);
            return default;
        }
    }

    public async Task SalvarAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(valor, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiracao
            };

            await cache.SetStringAsync(chave, json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao salvar chave {Chave} no cache Redis de catálogo.", chave);
        }
    }

    public async Task RemoverAsync(string chave, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.RemoveAsync(chave, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao remover chave {Chave} do cache Redis de catálogo.", chave);
        }
    }
}

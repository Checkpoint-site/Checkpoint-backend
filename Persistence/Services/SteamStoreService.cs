using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Domain.Dtos.Catalogo;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Persistence.Services;

public partial class SteamStoreService(
    HttpClient httpClient,
    ILogger<SteamStoreService> logger
) : ISteamStoreService
{
    private const string UrlPadraoCapaTemplate = "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/{0}/header.jpg";

    [GeneratedRegex(@"\b(19\d{2}|20\d{2})\b")]
    private static partial Regex RegexAnoQuatroDigitos();

    public async Task<IReadOnlyList<ItemBuscaSteamDto>> BuscarPorTermoAsync(string termo, CancellationToken cancellationToken = default)
    {
        var termoCodificado = Uri.EscapeDataString(termo.Trim());
        var url = $"storesearch/?term={termoCodificado}&l=brazilian&cc=BR";

        using var doc = await ObterJsonSteamAsync(url, cancellationToken);

        if (!doc.RootElement.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var resultado = new List<ItemBuscaSteamDto>();

        foreach (var item in itemsElement.EnumerateArray())
        {
            if (!item.TryGetProperty("id", out var idElement) || !idElement.TryGetInt32(out var steamAppId))
            {
                continue;
            }

            var nome = ObterString(item, "name") ?? $"App {steamAppId}";
            var imagem = ObterString(item, "tiny_image") ?? string.Format(UrlPadraoCapaTemplate, steamAppId);

            int? precoOriginal = null;
            int? precoFinal = null;

            if (item.TryGetProperty("price", out var priceElement) && priceElement.ValueKind == JsonValueKind.Object)
            {
                precoOriginal = ObterIntOpcional(priceElement, "initial");
                precoFinal = ObterIntOpcional(priceElement, "final");
            }

            var gratuito = (precoFinal ?? 0) == 0 && !item.TryGetProperty("price", out _);

            resultado.Add(new ItemBuscaSteamDto
            {
                SteamAppId = steamAppId,
                Nome = nome,
                ImagemCapaUrl = imagem,
                Gratuito = gratuito,
                PrecoOriginalCentavos = precoOriginal,
                PrecoFinalCentavos = precoFinal
            });
        }

        return resultado;
    }

    public async Task<DestaquesSteamDto> ObterDestaquesAsync(CancellationToken cancellationToken = default)
    {
        const string url = "featuredcategories/?l=brazilian&cc=BR";

        using var doc = await ObterJsonSteamAsync(url, cancellationToken);
        var root = doc.RootElement;

        return new DestaquesSteamDto
        {
            MaisVendidos = ExtrairListaDestaques(root, "top_sellers"),
            Lancamentos = ExtrairListaDestaques(root, "new_releases"),
            OfertasEspeciais = ExtrairListaDestaques(root, "specials"),
            EmBreve = ExtrairListaDestaques(root, "coming_soon")
        };
    }

    public async Task<DetalhesJogoSteamDto?> ObterDetalhesJogoAsync(int steamAppId, CancellationToken cancellationToken = default)
    {
        var url = $"appdetails?appids={steamAppId}&l=brazilian&cc=BR";

        using var doc = await ObterJsonSteamAsync(url, cancellationToken);
        var chaveApp = steamAppId.ToString();

        if (!doc.RootElement.TryGetProperty(chaveApp, out var envelope) || envelope.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!envelope.TryGetProperty("success", out var successElem) || successElem.ValueKind != JsonValueKind.True)
        {
            return null;
        }

        if (!envelope.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var nome = ObterString(data, "name") ?? $"App {steamAppId}";
        var descricaoCurta = WebUtility.HtmlDecode(ObterString(data, "short_description") ?? string.Empty).Trim();
        var imagemCapa = ObterString(data, "header_image") ?? string.Format(UrlPadraoCapaTemplate, steamAppId);

        var desenvolvedoras = ExtrairListaStrings(data, "developers");
        var publicadoras = ExtrairListaStrings(data, "publishers");

        var desenvolvedoraTexto = desenvolvedoras.Count > 0 ? string.Join(", ", desenvolvedoras) : "Desconhecida";
        var publicadoraTexto = publicadoras.Count > 0 ? string.Join(", ", publicadoras) : desenvolvedoraTexto;

        var generos = ExtrairGeneros(data);
        var gratuito = data.TryGetProperty("is_free", out var freeElem) && freeElem.ValueKind == JsonValueKind.True;

        bool emBreve = false;
        string? dataTexto = null;
        int? anoLancamento = null;

        if (data.TryGetProperty("release_date", out var releaseDate) && releaseDate.ValueKind == JsonValueKind.Object)
        {
            emBreve = releaseDate.TryGetProperty("coming_soon", out var comingElem) && comingElem.ValueKind == JsonValueKind.True;
            dataTexto = ObterString(releaseDate, "date");
            anoLancamento = ExtrairAnoLancamento(dataTexto);
        }

        int? notaMetacritic = null;
        if (data.TryGetProperty("metacritic", out var metacritic) && metacritic.ValueKind == JsonValueKind.Object)
        {
            notaMetacritic = ObterIntOpcional(metacritic, "score");
        }

        var totalRecomendacoes = 0;
        if (data.TryGetProperty("recommendations", out var recommendations) && recommendations.ValueKind == JsonValueKind.Object)
        {
            totalRecomendacoes = ObterIntOpcional(recommendations, "total") ?? 0;
        }

        int? precoOriginal = null;
        int? precoFinal = null;
        var percentualDesconto = 0;

        if (data.TryGetProperty("price_overview", out var priceOverview) && priceOverview.ValueKind == JsonValueKind.Object)
        {
            precoOriginal = ObterIntOpcional(priceOverview, "initial");
            precoFinal = ObterIntOpcional(priceOverview, "final");
            percentualDesconto = ObterIntOpcional(priceOverview, "discount_percent") ?? 0;
        }
        else if (gratuito)
        {
            precoOriginal = 0;
            precoFinal = 0;
        }

        return new DetalhesJogoSteamDto
        {
            SteamAppId = steamAppId,
            Nome = nome,
            DescricaoCurta = string.IsNullOrWhiteSpace(descricaoCurta) ? nome : descricaoCurta,
            ImagemCapaUrl = imagemCapa,
            Desenvolvedora = desenvolvedoraTexto,
            Publicadora = publicadoraTexto,
            Generos = generos,
            DataLancamentoTexto = dataTexto,
            AnoLancamento = anoLancamento,
            NotaMetacritic = notaMetacritic,
            TotalRecomendacoes = totalRecomendacoes,
            Gratuito = gratuito,
            EmBreve = emBreve,
            PrecoOriginalCentavos = precoOriginal,
            PrecoFinalCentavos = precoFinal,
            PercentualDesconto = percentualDesconto
        };
    }

    public static int? ExtrairAnoLancamento(string? textoData)
    {
        if (string.IsNullOrWhiteSpace(textoData))
        {
            return null;
        }

        var match = RegexAnoQuatroDigitos().Match(textoData);
        if (match.Success && int.TryParse(match.Value, out var ano))
        {
            return ano;
        }

        return null;
    }

    private async Task<JsonDocument> ObterJsonSteamAsync(string urlRelativa, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(urlRelativa, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Steam Storefront API retornou status {StatusCode} para {Url}.", (int)response.StatusCode, urlRelativa);
                throw new IntegrationException($"Falha ao consultar a Steam Storefront API (HTTP {(int)response.StatusCode}).");
            }

            var conteudo = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonDocument.Parse(conteudo);
        }
        catch (IntegrationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro de comunicação com a Steam Storefront API em {Url}.", urlRelativa);
            throw new IntegrationException("Não foi possível comunicar com a Steam Storefront API no momento.", ex);
        }
    }

    private static IReadOnlyList<ItemDestaqueSteamDto> ExtrairListaDestaques(JsonElement root, string secao)
    {
        if (!root.TryGetProperty(secao, out var secaoElem) || secaoElem.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        if (!secaoElem.TryGetProperty("items", out var itemsElem) || itemsElem.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var vistos = new HashSet<int>();
        var resultado = new List<ItemDestaqueSteamDto>();

        foreach (var item in itemsElem.EnumerateArray())
        {
            if (!item.TryGetProperty("id", out var idElem) || !idElem.TryGetInt32(out var steamAppId))
            {
                continue;
            }

            if (!vistos.Add(steamAppId))
            {
                continue;
            }

            var nome = ObterString(item, "name") ?? $"App {steamAppId}";
            var imagem = ObterString(item, "large_capsule_image")
                ?? ObterString(item, "header_image")
                ?? string.Format(UrlPadraoCapaTemplate, steamAppId);

            var precoOriginal = ObterIntOpcional(item, "original_price");
            var precoFinal = ObterIntOpcional(item, "final_price");
            var desconto = ObterIntOpcional(item, "discount_percent") ?? 0;
            var gratuito = (precoFinal ?? 0) == 0 && (precoOriginal ?? 0) == 0;

            resultado.Add(new ItemDestaqueSteamDto
            {
                SteamAppId = steamAppId,
                Nome = nome,
                ImagemCapaUrl = imagem,
                Gratuito = gratuito,
                PrecoOriginalCentavos = precoOriginal,
                PrecoFinalCentavos = precoFinal,
                PercentualDesconto = desconto
            });
        }

        return resultado;
    }

    private static List<string> ExtrairListaStrings(JsonElement parent, string nomePropriedade)
    {
        if (!parent.TryGetProperty(nomePropriedade, out var arrayElem) || arrayElem.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var valores = new List<string>();
        foreach (var elem in arrayElem.EnumerateArray())
        {
            if (elem.ValueKind == JsonValueKind.String)
            {
                var texto = elem.GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    valores.Add(texto);
                }
            }
        }

        return valores;
    }

    private static List<Generos> ExtrairGeneros(JsonElement data)
    {
        if (!data.TryGetProperty("genres", out var genresElem) || genresElem.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var generos = new List<Generos>();

        foreach (var item in genresElem.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("id", out var idElem))
            {
                continue;
            }

            int idGenero;
            if (idElem.ValueKind == JsonValueKind.String)
            {
                if (!int.TryParse(idElem.GetString(), out idGenero))
                {
                    continue;
                }
            }
            else if (idElem.ValueKind == JsonValueKind.Number)
            {
                if (!idElem.TryGetInt32(out idGenero))
                {
                    continue;
                }
            }
            else
            {
                continue;
            }

            if (Enum.IsDefined(typeof(Generos), idGenero))
            {
                var generoConvertido = (Generos)idGenero;
                if (!generos.Contains(generoConvertido))
                {
                    generos.Add(generoConvertido);
                }
            }
        }

        return generos;
    }

    private static string? ObterString(JsonElement elem, string prop)
    {
        if (elem.TryGetProperty(prop, out var valor) && valor.ValueKind == JsonValueKind.String)
        {
            return valor.GetString();
        }

        return null;
    }

    private static int? ObterIntOpcional(JsonElement elem, string prop)
    {
        if (elem.TryGetProperty(prop, out var valor) && valor.ValueKind == JsonValueKind.Number && valor.TryGetInt32(out var numero))
        {
            return numero;
        }

        return null;
    }
}

using System.Net;
using System.Text;
using Domain.Enum;
using Domain.Exceptions.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Persistence.Services;

namespace Test.Persistence.Services;

public class SteamStoreServiceTests
{
    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }

    [Fact]
    public async Task ObterDetalhesJogoAsync_ComPayloadRealPtBr_ExtraiGenerosAnoAutorEPrecoCorretamente()
    {
        const string payloadJson = """
        {
          "292030": {
            "success": true,
            "data": {
              "type": "game",
              "name": "The Witcher 3: Wild Hunt",
              "steam_appid": 292030,
              "is_free": false,
              "short_description": "Você é Geralt de Rívia, mercenário matador de monstros.",
              "header_image": "https://cdn.akamai.steamstatic.com/steam/apps/292030/header.jpg",
              "developers": ["CD PROJEKT RED"],
              "publishers": ["CD PROJEKT RED"],
              "price_overview": {
                "currency": "BRL",
                "initial": 12999,
                "final": 3249,
                "discount_percent": 75
              },
              "metacritic": {
                "score": 93
              },
              "genres": [
                { "id": "3", "description": "RPG" },
                { "id": "25", "description": "Aventura" },
                { "id": "999", "description": "Tag Desconhecida" }
              ],
              "recommendations": {
                "total": 725000
              },
              "release_date": {
                "coming_soon": false,
                "date": "18/mai./2015"
              }
            }
          }
        }
        """;

        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://store.steampowered.com/api/")
        };

        var service = new SteamStoreService(httpClient, NullLogger<SteamStoreService>.Instance);

        var detalhes = await service.ObterDetalhesJogoAsync(292030);

        Assert.NotNull(detalhes);
        Assert.Equal("The Witcher 3: Wild Hunt", detalhes.Nome);
        Assert.Equal("CD PROJEKT RED", detalhes.Desenvolvedora);
        Assert.Equal(2015, detalhes.AnoLancamento);
        Assert.Equal(93, detalhes.NotaMetacritic);
        Assert.Equal(75, detalhes.PercentualDesconto);
        Assert.Contains(Generos.Rpg, detalhes.Generos);
        Assert.Contains(Generos.Aventura, detalhes.Generos);
        Assert.Equal(2, detalhes.Generos.Count);
    }

    [Fact]
    public async Task BuscarJogosAsync_QuandoSteamRetornaErroHttp_LancaIntegrationException()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://store.steampowered.com/api/")
        };

        var service = new SteamStoreService(httpClient, NullLogger<SteamStoreService>.Instance);

        await Assert.ThrowsAsync<IntegrationException>(() => service.BuscarJogosAsync("witcher"));
    }
}

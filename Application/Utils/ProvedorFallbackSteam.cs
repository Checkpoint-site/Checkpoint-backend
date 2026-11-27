using Application.Dtos.Catalogo.Responses;
using Application.Mappers;
using Application.Services;
using Application.Strategies.Algoritmos;
using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Respost;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;

namespace Application.Utils;

public static class ProvedorFallbackSteam
{
    public const string ChavePoolFallback = "catalogo:fallback:pool:v1";
    private const string ChaveDestaquesSteam = "catalogo:destaques:steam";
    private const int MaximoCandidatosParaHidratar = 16;
    private const int ConcorrenciaMaximaDetalhes = 6;
    private const int MinimoJogosHidratadosDesejado = 8;

    private static readonly TimeSpan TtlPoolFallback = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan TtlDestaques = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan TtlOfertaTemporaria = TimeSpan.FromMinutes(30);

    private static readonly string[] TermosDescobertaRelevantes =
    [
        "rpg",
        "souls",
        "indie",
        "roguelike",
        "hollow",
        "witcher",
        "cyberpunk",
        "dragon",
        "resident",
        "baldur",
        "persona",
        "metroidvania",
        "strategy",
        "survival",
        "fantasy",
        "knight"
    ];

    private sealed record CandidatoBrutoSteam(
        int SteamAppId,
        string Nome,
        string ImagemCapaUrl,
        bool Gratuito,
        int? PrecoOriginalCentavos,
        int? PrecoFinalCentavos,
        int PercentualDesconto,
        double PesoFonte);

    public static async Task<IReadOnlyList<ItemPoolFallbackSteamDto>> ObterPoolAsync(
        ICatalogoRepository catalogoRepository,
        ISteamStoreService steamStoreService,
        ICatalogoCacheService cacheService,
        CancellationToken cancellationToken = default)
    {
        var emCache = await cacheService.ObterAsync<List<ItemPoolFallbackSteamDto>>(ChavePoolFallback, cancellationToken);
        if (emCache is { Count: > 0 })
        {
            return emCache;
        }

        var candidatosPorAppId = new Dictionary<int, CandidatoBrutoSteam>();
        DestaquesSteamDto? destaques = null;

        try
        {
            destaques = await cacheService.ObterAsync<DestaquesSteamDto>(ChaveDestaquesSteam, cancellationToken);
            if (destaques is null)
            {
                destaques = await steamStoreService.ObterDestaquesAsync(cancellationToken);
                await cacheService.SalvarAsync(ChaveDestaquesSteam, destaques, TtlDestaques, cancellationToken);
            }

            RegistrarDestaques(candidatosPorAppId, destaques.MaisVendidos, pesoFonte: 1.0);
            RegistrarDestaques(candidatosPorAppId, destaques.OfertasEspeciais, pesoFonte: 0.92);
            RegistrarDestaques(candidatosPorAppId, destaques.Lancamentos, pesoFonte: 0.88);
        }
        catch (IntegrationException)
        {
            // Prossegue tentando buscas temáticas caso o endpoint de categorias oscile.
        }

        var termosSorteados = TermosDescobertaRelevantes
            .OrderBy(_ => Random.Shared.Next())
            .Take(3)
            .ToList();

        var tarefasBusca = termosSorteados
            .Select(termo => BuscarTermoSeguroAsync(steamStoreService, termo, cancellationToken))
            .ToList();

        var resultadosBusca = await Task.WhenAll(tarefasBusca);
        foreach (var lista in resultadosBusca)
        {
            for (var i = 0; i < Math.Min(lista.Count, 6); i++)
            {
                var item = lista[i];
                if (item.SteamAppId <= 0)
                {
                    continue;
                }

                var peso = i < 2 ? 0.90 : 0.84;
                var desconto = CalcularPercentualDesconto(item.PrecoOriginalCentavos, item.PrecoFinalCentavos);
                RegistrarCandidato(
                    candidatosPorAppId,
                    new CandidatoBrutoSteam(
                        item.SteamAppId,
                        item.Nome,
                        item.ImagemCapaUrl,
                        item.Gratuito,
                        item.PrecoOriginalCentavos,
                        item.PrecoFinalCentavos,
                        desconto,
                        peso));
            }
        }

        if (candidatosPorAppId.Count == 0 && destaques is { EmBreve.Count: > 0 })
        {
            RegistrarDestaques(candidatosPorAppId, destaques.EmBreve, pesoFonte: 0.72);
        }

        if (candidatosPorAppId.Count == 0)
        {
            return [];
        }

        var selecionados = candidatosPorAppId.Values
            .OrderByDescending(c => c.PesoFonte * (0.70 + (Random.Shared.NextDouble() * 0.60)))
            .Take(MaximoCandidatosParaHidratar)
            .ToList();

        var idsSelecionados = selecionados.Select(c => c.SteamAppId).ToList();
        var existentesNoBanco = await catalogoRepository.ObterPorSteamAppIdsAsync(idsSelecionados, cancellationToken);
        var mapaExistentes = existentesNoBanco.ToDictionary(j => j.SteamAppId);

        var poolLancados = new List<ItemPoolFallbackSteamDto>();
        var poolEmBreve = new List<ItemPoolFallbackSteamDto>();
        var candidatosSemDetalhes = new List<CandidatoBrutoSteam>();

        foreach (var candidato in selecionados)
        {
            if (mapaExistentes.TryGetValue(candidato.SteamAppId, out var jogoBanco))
            {
                var oferta = CriarOfertaDeCandidato(candidato);
                poolLancados.Add(new ItemPoolFallbackSteamDto
                {
                    Jogo = jogoBanco,
                    Oferta = oferta,
                    PesoRelevanciaFonte = candidato.PesoFonte
                });
            }
        }

        var paraHidratar = selecionados
            .Where(c => !mapaExistentes.ContainsKey(c.SteamAppId))
            .ToList();

        using var semaforo = new SemaphoreSlim(ConcorrenciaMaximaDetalhes, ConcorrenciaMaximaDetalhes);
        var tarefasDetalhes = paraHidratar
            .Select(async candidato =>
            {
                await semaforo.WaitAsync(cancellationToken);
                try
                {
                    var detalhes = await steamStoreService.ObterDetalhesJogoAsync(candidato.SteamAppId, cancellationToken);
                    return (Candidato: candidato, Detalhes: detalhes);
                }
                catch (IntegrationException)
                {
                    return (Candidato: candidato, Detalhes: (DetalhesJogoSteamDto?)null);
                }
                finally
                {
                    semaforo.Release();
                }
            })
            .ToList();

        var hidratados = await Task.WhenAll(tarefasDetalhes);

        foreach (var (candidato, detalhes) in hidratados)
        {
            if (detalhes is null)
            {
                candidatosSemDetalhes.Add(candidato);
                continue;
            }

            var oferta = CatalogoService.ExtrairOfertaTemporaria(detalhes);
            await cacheService.SalvarAsync(
                CatalogoService.ObterChaveOferta(detalhes.SteamAppId),
                oferta,
                TtlOfertaTemporaria,
                cancellationToken);

            var itemPool = new ItemPoolFallbackSteamDto
            {
                Jogo = ConverterDetalhesSteamParaJogoDto(detalhes),
                Oferta = oferta,
                PesoRelevanciaFonte = candidato.PesoFonte
            };

            if (detalhes.EmBreve)
            {
                poolEmBreve.Add(itemPool);
            }
            else
            {
                poolLancados.Add(itemPool);
            }
        }

        var resultadoFinal = new List<ItemPoolFallbackSteamDto>(poolLancados);

        if (resultadoFinal.Count < MinimoJogosHidratadosDesejado)
        {
            foreach (var candidato in candidatosSemDetalhes)
            {
                if (resultadoFinal.Count >= MinimoJogosHidratadosDesejado)
                {
                    break;
                }

                var oferta = CriarOfertaDeCandidato(candidato);
                await cacheService.SalvarAsync(
                    CatalogoService.ObterChaveOferta(candidato.SteamAppId),
                    oferta,
                    TtlOfertaTemporaria,
                    cancellationToken);

                resultadoFinal.Add(new ItemPoolFallbackSteamDto
                {
                    Jogo = new JogoCatalogoRespostDto
                    {
                        IdJogo = 0,
                        SteamAppId = candidato.SteamAppId,
                        Nome = candidato.Nome,
                        Descricao = $"Explore {candidato.Nome} na Steam e registre sua jornada na Fogueira.",
                        Imagem = candidato.ImagemCapaUrl,
                        Desenvolvedora = "Steam",
                        Publicadora = "Steam",
                        Generos = [],
                        Gratuito = candidato.Gratuito,
                        EmBreve = false,
                        SincronizadoEm = DateTime.UtcNow
                    },
                    Oferta = oferta,
                    PesoRelevanciaFonte = candidato.PesoFonte * 0.85
                });
            }
        }

        if (resultadoFinal.Count < 4 && poolEmBreve.Count > 0)
        {
            resultadoFinal.AddRange(poolEmBreve.Take(MinimoJogosHidratadosDesejado - resultadoFinal.Count));
        }

        if (resultadoFinal.Count > 0)
        {
            await cacheService.SalvarAsync(ChavePoolFallback, resultadoFinal, TtlPoolFallback, cancellationToken);
        }

        return resultadoFinal;
    }

    public static IReadOnlyList<JogoResumoResponseDto> OrdenarComRelevanciaEAleatoriedade(
        IEnumerable<(JogoCatalogoRespostDto Jogo, double PesoRelevancia)> candidatos,
        AlgoritmoCuradoriaCatalogo algoritmo,
        ContextoCuradoria contexto,
        Random? random = null)
    {
        var rng = random ?? Random.Shared;

        return candidatos
            .GroupBy(c => c.Jogo.SteamAppId)
            .Select(g => g.OrderByDescending(x => x.Jogo.IdJogo).First())
            .Select(c =>
            {
                var logger = c.Jogo.IdJogo > 0 ? contexto.ObterJogoLogger(c.Jogo.IdJogo) : null;
                var oferta = contexto.ObterOferta(c.Jogo.SteamAppId);
                var mediaFogueiras = logger is not null
                    ? ContextoCuradoria.CalcularMediaFogueiras(logger)
                    : c.Jogo.MediaFogueiras;
                var totalAvaliacoes = logger is { TotalAvaliacoes: > 0 }
                    ? (int)logger.TotalAvaliacoes
                    : c.Jogo.TotalAvaliacoes;

                var relevanciaBase = algoritmo.CalcularPontuacao(c.Jogo, c.PesoRelevancia, contexto);

                if (c.Jogo.NotaMetacritic is >= 80)
                {
                    relevanciaBase += 6.0;
                }

                if (c.Jogo.TotalRecomendacoes >= 5000)
                {
                    relevanciaBase += 5.0;
                }

                var multiplicadorAleatorio = 0.78 + (rng.NextDouble() * 0.44);
                var jitterAditivo = rng.NextDouble() * 12.0;
                var pontuacaoFinal = Math.Round((relevanciaBase * multiplicadorAleatorio) + jitterAditivo, 2);

                return CatalogoMapper.ToResumoResponse(
                    c.Jogo,
                    pontuacaoFinal,
                    mediaFogueiras,
                    oferta,
                    totalAvaliacoes);
            })
            .OrderByDescending(j => j.PontuacaoCuradoria)
            .ThenByDescending(j => j.NotaMetacritic ?? 0)
            .ThenBy(j => j.Nome)
            .ToList();
    }

    private static async Task<IReadOnlyList<ItemBuscaSteamDto>> BuscarTermoSeguroAsync(
        ISteamStoreService steamStoreService,
        string termo,
        CancellationToken cancellationToken)
    {
        try
        {
            return await steamStoreService.BuscarJogosAsync(termo, cancellationToken);
        }
        catch (IntegrationException)
        {
            return [];
        }
    }

    private static void RegistrarDestaques(
        Dictionary<int, CandidatoBrutoSteam> destino,
        IReadOnlyList<ItemDestaqueSteamDto> itens,
        double pesoFonte)
    {
        foreach (var item in itens)
        {
            if (item.SteamAppId <= 0)
            {
                continue;
            }

            RegistrarCandidato(
                destino,
                new CandidatoBrutoSteam(
                    item.SteamAppId,
                    item.Nome,
                    item.ImagemCapaUrl,
                    item.Gratuito,
                    item.PrecoOriginalCentavos,
                    item.PrecoFinalCentavos,
                    item.PercentualDesconto,
                    pesoFonte));
        }
    }

    private static void RegistrarCandidato(
        Dictionary<int, CandidatoBrutoSteam> destino,
        CandidatoBrutoSteam novo)
    {
        if (!destino.TryGetValue(novo.SteamAppId, out var existente) || novo.PesoFonte > existente.PesoFonte)
        {
            destino[novo.SteamAppId] = novo;
        }
    }

    private static int CalcularPercentualDesconto(int? precoOriginal, int? precoFinal)
    {
        if (precoOriginal is > 0 && precoFinal is >= 0 && precoFinal < precoOriginal)
        {
            return (int)Math.Round((1.0 - ((double)precoFinal.Value / precoOriginal.Value)) * 100.0);
        }

        return 0;
    }

    private static OfertaJogoSteamDto CriarOfertaDeCandidato(CandidatoBrutoSteam candidato) => new()
    {
        SteamAppId = candidato.SteamAppId,
        Gratuito = candidato.Gratuito,
        PrecoOriginalCentavos = candidato.PrecoOriginalCentavos,
        PrecoFinalCentavos = candidato.PrecoFinalCentavos,
        PercentualDesconto = candidato.PercentualDesconto,
        ConsultadoEm = DateTime.UtcNow
    };

    private static JogoCatalogoRespostDto ConverterDetalhesSteamParaJogoDto(DetalhesJogoSteamDto detalhes) => new()
    {
        IdJogo = 0,
        SteamAppId = detalhes.SteamAppId,
        Nome = detalhes.Nome,
        Descricao = detalhes.DescricaoCurta,
        Imagem = detalhes.ImagemCapaUrl,
        Desenvolvedora = detalhes.Desenvolvedora,
        Publicadora = detalhes.Publicadora,
        Generos = detalhes.Generos.Distinct().ToList(),
        DataLancamentoTexto = detalhes.DataLancamentoTexto,
        AnoLancamento = detalhes.AnoLancamento,
        NotaMetacritic = detalhes.NotaMetacritic,
        TotalRecomendacoes = detalhes.TotalRecomendacoes,
        Gratuito = detalhes.Gratuito,
        EmBreve = detalhes.EmBreve,
        MediaFogueiras = 0.0,
        TotalAvaliacoes = 0,
        SincronizadoEm = DateTime.UtcNow
    };
}

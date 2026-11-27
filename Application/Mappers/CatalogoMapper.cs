using Application.Dtos.Catalogo.Responses;
using Application.Strategies.Algoritmos;
using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Loggers.Respost;
using Domain.Dtos.Posts.Respost;

namespace Application.Mappers;

public static class CatalogoMapper
{
    public static ItemVitrineResponseDto ToItemVitrineResponse(
        ItemDestaqueSteamDto item,
        double pontuacaoCuradoria,
        JogoCatalogoRespostDto? jogoLocal = null,
        JogoLoggerRespostDto? logger = null)
    {
        var media = logger is not null
            ? ContextoCuradoria.CalcularMediaFogueiras(logger)
            : (jogoLocal?.MediaFogueiras ?? 0.0);

        var totalAvaliacoes = logger is { TotalAvaliacoes: > 0 }
            ? (int)logger.TotalAvaliacoes
            : (jogoLocal?.TotalAvaliacoes ?? (int)(logger?.PresencaPost ?? 0UL));

        return new ItemVitrineResponseDto
        {
            IdJogo = jogoLocal?.IdJogo ?? 0,
            SteamAppId = item.SteamAppId,
            Nome = jogoLocal?.Nome ?? item.Nome,
            Descricao = jogoLocal?.Descricao ?? item.Nome,
            ImagemCapaUrl = jogoLocal?.Imagem ?? item.ImagemCapaUrl,
            Desenvolvedora = jogoLocal?.Desenvolvedora ?? "Steam",
            AnoLancamento = jogoLocal?.AnoLancamento,
            Generos = jogoLocal?.Generos.Select(g => g.ToString()).ToList() ?? [],
            Gratuito = item.Gratuito || jogoLocal?.Gratuito == true,
            PrecoOriginalCentavos = item.PrecoOriginalCentavos,
            PrecoFinalCentavos = item.PrecoFinalCentavos,
            PercentualDesconto = item.PercentualDesconto,
            PontuacaoCuradoria = Math.Round(pontuacaoCuradoria, 2),
            MediaFogueiras = Math.Round(media, 2),
            TotalAvaliacoes = totalAvaliacoes
        };
    }

    public static JogoResumoResponseDto ToResumoResponse(
        JogoCatalogoRespostDto jogo,
        double pontuacaoCuradoria = 0.0,
        double mediaFogueiras = 0.0,
        OfertaJogoSteamDto? oferta = null,
        int? totalAvaliacoesOverride = null)
    {
        var mediaFinal = mediaFogueiras > 0 ? mediaFogueiras : jogo.MediaFogueiras;
        var totalFinal = totalAvaliacoesOverride ?? jogo.TotalAvaliacoes;

        return new JogoResumoResponseDto
        {
            IdJogo = jogo.IdJogo,
            SteamAppId = jogo.SteamAppId,
            Nome = jogo.Nome,
            Descricao = jogo.Descricao,
            Imagem = jogo.Imagem,
            Desenvolvedora = jogo.Desenvolvedora,
            Publicadora = jogo.Publicadora,
            AnoLancamento = jogo.AnoLancamento,
            NotaMetacritic = jogo.NotaMetacritic,
            Gratuito = jogo.Gratuito || oferta?.Gratuito == true,
            EmBreve = jogo.EmBreve,
            PrecoOriginalCentavos = oferta?.PrecoOriginalCentavos,
            PrecoFinalCentavos = oferta?.PrecoFinalCentavos,
            PercentualDesconto = oferta?.PercentualDesconto ?? 0,
            PontuacaoCuradoria = Math.Round(pontuacaoCuradoria, 2),
            MediaFogueiras = Math.Round(mediaFinal, 2),
            TotalAvaliacoes = totalFinal,
            Generos = jogo.Generos.Select(g => g.ToString()).ToList()
        };
    }

    public static JogoResumoResponseDto ToResumoResponse(JogoCuradoResultado resultado)
    {
        return ToResumoResponse(resultado.Jogo, resultado.PontuacaoCuradoria, resultado.MediaFogueiras, resultado.Oferta);
    }

    public static JogoDetalhesResponseDto ToDetalhesResponse(
        JogoCatalogoRespostDto jogo,
        JogoLoggerRespostDto? logger,
        OfertaJogoSteamDto? oferta = null,
        IReadOnlyList<FaixaDistribuicaoFogueirasDto>? distribuicao = null,
        IReadOnlyList<JogoResumoResponseDto>? relacionados = null)
    {
        var mediaFogueiras = logger is not null
            ? ContextoCuradoria.CalcularMediaFogueiras(logger)
            : jogo.MediaFogueiras;

        var totalAvaliacoes = logger is { TotalAvaliacoes: > 0 }
            ? (int)logger.TotalAvaliacoes
            : (jogo.TotalAvaliacoes > 0 ? jogo.TotalAvaliacoes : (int)(logger?.PresencaPost ?? 0UL));

        var metricas = new JogoMetricasLoggerResponseDto
        {
            QuantidadeCurtida = logger?.QuantidadeCurtida ?? 0UL,
            PresencaPost = logger?.PresencaPost ?? 0UL,
            PresencaComentario = logger?.PresencaComentario ?? 0UL,
            PresencaFavoritos = logger?.PresencaFavoritos ?? 0UL,
            PresencaPretendeJogar = logger?.PresencaPretendeJogar ?? 0UL,
            SomaFogueiras = logger?.SomaFogueiras ?? 0.0,
            TotalAvaliacoes = logger?.TotalAvaliacoes ?? (ulong)Math.Max(0, totalAvaliacoes),
            MediaFogueiras = mediaFogueiras
        };

        var faixas = (distribuicao ?? CriarDistribuicaoVazia())
            .Select(f => new FaixaDistribuicaoFogueirasResponseDto
            {
                Estrelas = f.Estrelas,
                Quantidade = f.Quantidade,
                Percentual = f.Percentual
            })
            .ToList();

        return new JogoDetalhesResponseDto
        {
            IdJogo = jogo.IdJogo,
            SteamAppId = jogo.SteamAppId,
            Nome = jogo.Nome,
            Descricao = jogo.Descricao,
            Imagem = jogo.Imagem,
            Desenvolvedora = jogo.Desenvolvedora,
            Publicadora = jogo.Publicadora,
            Generos = jogo.Generos.Select(g => g.ToString()).ToList(),
            DataLancamentoTexto = jogo.DataLancamentoTexto,
            AnoLancamento = jogo.AnoLancamento,
            NotaMetacritic = jogo.NotaMetacritic,
            TotalRecomendacoes = jogo.TotalRecomendacoes,
            Gratuito = jogo.Gratuito || oferta?.Gratuito == true,
            EmBreve = jogo.EmBreve,
            PrecoOriginalCentavos = oferta?.PrecoOriginalCentavos,
            PrecoFinalCentavos = oferta?.PrecoFinalCentavos,
            PercentualDesconto = oferta?.PercentualDesconto ?? 0,
            SincronizadoEm = jogo.SincronizadoEm,
            MediaFogueiras = mediaFogueiras,
            TotalAvaliacoes = totalAvaliacoes,
            DistribuicaoFogueiras = faixas,
            JogosRelacionados = relacionados ?? [],
            EngajamentoComunidade = metricas
        };
    }

    private static IReadOnlyList<FaixaDistribuicaoFogueirasDto> CriarDistribuicaoVazia() =>
        new[] { 5, 4, 3, 2, 1 }
            .Select(estrelas => new FaixaDistribuicaoFogueirasDto
            {
                Estrelas = estrelas,
                Quantidade = 0,
                Percentual = 0
            })
            .ToList();
}

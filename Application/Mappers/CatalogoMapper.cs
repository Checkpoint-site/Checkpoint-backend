using Application.Dtos.Catalogo.Responses;
using Application.Strategies.Algoritmos;
using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Loggers.Respost;
using Riok.Mapperly.Abstractions;

namespace Application.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class CatalogoMapper
{
    [MapperIgnoreTarget(nameof(ItemVitrineResponseDto.PontuacaoCuradoria))]
    private static partial ItemVitrineResponseDto MapItemVitrineBase(ItemDestaqueSteamDto item);

    public static ItemVitrineResponseDto ToItemVitrineResponse(ItemDestaqueSteamDto item, double pontuacaoCuradoria)
    {
        return MapItemVitrineBase(item) with
        {
            PontuacaoCuradoria = Math.Round(pontuacaoCuradoria, 2)
        };
    }

    public static JogoResumoResponseDto ToResumoResponse(
        JogoCatalogoRespostDto jogo,
        double pontuacaoCuradoria = 0.0,
        double mediaFogueiras = 0.0,
        OfertaJogoSteamDto? oferta = null)
    {
        return new JogoResumoResponseDto
        {
            IdJogo = jogo.IdJogo,
            SteamAppId = jogo.SteamAppId,
            Nome = jogo.Nome,
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
            MediaFogueiras = Math.Round(mediaFogueiras, 2),
            Generos = jogo.Generos.Select(g => g.ToString()).ToList()
        };
    }

    public static JogoResumoResponseDto ToResumoResponse(JogoCuradoResultado resultado)
    {
        return ToResumoResponse(resultado.Jogo, resultado.PontuacaoCuradoria, resultado.MediaFogueiras, resultado.Oferta);
    }

    public static JogoDetalhesResponseDto ToDetalhesResponse(JogoCatalogoRespostDto jogo, JogoLoggerRespostDto? logger, OfertaJogoSteamDto? oferta = null)
    {
        var metricas = new JogoMetricasLoggerResponseDto
        {
            QuantidadeCurtida = logger?.QuantidadeCurtida ?? 0UL,
            PresencaPost = logger?.PresencaPost ?? 0UL,
            PresencaComentario = logger?.PresencaComentario ?? 0UL,
            PresencaFavoritos = logger?.PresencaFavoritos ?? 0UL,
            PresencaPretendeJogar = logger?.PresencaPretendeJogar ?? 0UL,
            SomaFogueiras = logger?.SomaFogueiras ?? 0UL,
            MediaFogueiras = ContextoCuradoria.CalcularMediaFogueiras(logger)
        };

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
            EngajamentoComunidade = metricas
        };
    }
}

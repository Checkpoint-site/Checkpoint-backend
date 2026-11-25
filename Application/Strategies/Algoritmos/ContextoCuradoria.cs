using Domain.Dtos.Catalogo;
using Domain.Enum;
using Domain.Model;

namespace Application.Strategies.Algoritmos;

public record JogoCuradoResultado(
    Jogo Jogo,
    double PontuacaoCuradoria,
    double MediaFogueiras,
    OfertaJogoSteamDto? Oferta = null
);

public class ContextoCuradoria
{
    public IReadOnlyDictionary<int, JogoLogger> JogoLoggersPorJogo { get; init; } = new Dictionary<int, JogoLogger>();
    public IReadOnlyDictionary<Generos, GeneroLogger> GeneroLoggersGlobais { get; init; } = new Dictionary<Generos, GeneroLogger>();
    public IReadOnlyDictionary<Generos, GeneroLogger> GeneroLoggersUsuario { get; init; } = new Dictionary<Generos, GeneroLogger>();
    public IReadOnlyDictionary<int, OfertaJogoSteamDto> OfertasPorSteamAppId { get; init; } = new Dictionary<int, OfertaJogoSteamDto>();
    public bool UsuarioAutenticado { get; init; }

    public JogoLogger? ObterJogoLogger(int idJogo)
    {
        return JogoLoggersPorJogo.TryGetValue(idJogo, out var logger) ? logger : null;
    }

    public OfertaJogoSteamDto? ObterOferta(int steamAppId)
    {
        return OfertasPorSteamAppId.TryGetValue(steamAppId, out var oferta) ? oferta : null;
    }

    public static double CalcularMediaFogueiras(JogoLogger? logger)
    {
        if (logger is null || logger.PresencaPost == 0)
        {
            return 0.0;
        }

        var media = (double)logger.SomaFogueiras / logger.PresencaPost;
        return Math.Round(Math.Clamp(media, 0.0, 5.0), 2);
    }

    public static double CalcularForcaJogoLogger(JogoLogger? logger)
    {
        if (logger is null)
        {
            return 0.0;
        }

        var mediaFogueiras = CalcularMediaFogueiras(logger);

        return (logger.PresencaFavoritos * 6.0)
             + (logger.PresencaPost * 5.0)
             + (mediaFogueiras * 10.0)
             + (logger.PresencaPretendeJogar * 3.0)
             + (logger.PresencaComentario * 2.0)
             + (logger.QuantidadeCurtida * 1.5);
    }

    public static double CalcularForcaGeneroLogger(GeneroLogger? logger)
    {
        if (logger is null)
        {
            return 0.0;
        }

        return (logger.PresencaFavoritos * 5.0)
             + (logger.PresencaPost * 4.0)
             + (logger.PresencaPretendeJogar * 3.0)
             + (logger.PresencaComentario * 2.0)
             + (logger.QuantidadeCurtida * 1.0);
    }

    public double CalcularAfinidadeGenerosDoJogo(Jogo jogo)
    {
        if (jogo.Generos.Count == 0)
        {
            return 0.0;
        }

        double soma = 0.0;

        foreach (var genero in jogo.Generos)
        {
            GeneroLoggersGlobais.TryGetValue(genero, out var globalLogger);
            var scoreGlobal = CalcularForcaGeneroLogger(globalLogger);

            if (UsuarioAutenticado)
            {
                GeneroLoggersUsuario.TryGetValue(genero, out var userLogger);
                var scoreUsuario = CalcularForcaGeneroLogger(userLogger);
                soma += (scoreUsuario * 0.70) + (scoreGlobal * 0.30);
            }
            else
            {
                soma += scoreGlobal;
            }
        }

        return soma / jogo.Generos.Count;
    }
}

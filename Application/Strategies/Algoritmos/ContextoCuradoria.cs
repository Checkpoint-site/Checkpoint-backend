using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Loggers.Respost;
using Domain.Enum;

namespace Application.Strategies.Algoritmos;

public class ContextoCuradoria
{
    public IReadOnlyDictionary<int, JogoLoggerRespostDto> JogoLoggersPorJogo { get; init; } = new Dictionary<int, JogoLoggerRespostDto>();
    public IReadOnlyDictionary<Generos, GeneroLoggerRespostDto> GeneroLoggers { get; init; } = new Dictionary<Generos, GeneroLoggerRespostDto>();
    public IReadOnlyDictionary<int, OfertaJogoSteamDto> OfertasPorSteamAppId { get; init; } = new Dictionary<int, OfertaJogoSteamDto>();

    public JogoLoggerRespostDto? ObterJogoLogger(int idJogo)
    {
        return JogoLoggersPorJogo.TryGetValue(idJogo, out var logger) ? logger : null;
    }

    public OfertaJogoSteamDto? ObterOferta(int steamAppId)
    {
        return OfertasPorSteamAppId.TryGetValue(steamAppId, out var oferta) ? oferta : null;
    }

    public static double CalcularMediaFogueiras(JogoLoggerRespostDto? logger)
    {
        if (logger is null)
        {
            return 0.0;
        }

        var divisor = logger.TotalAvaliacoes > 0 ? logger.TotalAvaliacoes : logger.PresencaPost;
        if (divisor == 0)
        {
            return logger.MediaFogueiras > 0
                ? Math.Round(Math.Clamp(logger.MediaFogueiras, 0.0, 5.0), 2)
                : 0.0;
        }

        var media = logger.SomaFogueiras / divisor;
        return Math.Round(Math.Clamp(media, 0.0, 5.0), 2);
    }

    public static double CalcularForcaJogoLogger(JogoLoggerRespostDto? logger)
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

    public static double CalcularForcaGeneroLogger(GeneroLoggerRespostDto? logger)
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

    public static double CalcularForcaUsuarioLogger(UsuarioLoggerRespostDto? logger)
    {
        if (logger is null)
        {
            return 0.0;
        }

        return CalcularForcaUsuarioLogger(
            logger.CurtidasRecebidas,
            logger.PresencaPost,
            logger.QuantidadeSeguidores,
            logger.PresencaComentario,
            logger.PresencaFavoritos,
            logger.PossuiFotoPerfil);
    }

    public static double CalcularForcaUsuarioLogger(
        ulong curtidasRecebidas,
        ulong presencaPost,
        ulong quantidadeSeguidores,
        ulong presencaComentario,
        ulong presencaFavoritos,
        bool possuiFotoPerfil)
    {
        const double bonusFotoPerfil = 10.0;

        return (curtidasRecebidas * 4.0)
             + (presencaPost * 5.0)
             + (quantidadeSeguidores * 3.0)
             + (presencaComentario * 2.0)
             + (presencaFavoritos * 1.0)
             + (possuiFotoPerfil ? bonusFotoPerfil : 0.0);
    }

    public double CalcularAfinidadeGenerosDoJogo(JogoCatalogoRespostDto jogo)
    {
        if (jogo.Generos.Count == 0)
        {
            return 0.0;
        }

        double soma = 0.0;

        foreach (var genero in jogo.Generos)
        {
            GeneroLoggers.TryGetValue(genero, out var logger);
            soma += CalcularForcaGeneroLogger(logger);
        }

        return soma / jogo.Generos.Count;
    }
}

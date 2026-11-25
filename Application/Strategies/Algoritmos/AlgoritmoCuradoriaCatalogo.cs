using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;

namespace Application.Strategies.Algoritmos;

public abstract class AlgoritmoCuradoriaCatalogo
{
    public abstract TipoAlgoritmoCuradoria TipoAlgoritmo { get; }
    public abstract string ChaveExibicao { get; }

    public abstract double CalcularPontuacao(JogoCatalogoRespostDto jogo, double afinidadeBusca, ContextoCuradoria contexto);

    public IReadOnlyList<JogoCuradoResultado> Ordenar(
        IEnumerable<(JogoCatalogoRespostDto Jogo, double Afinidade)> candidatos,
        ContextoCuradoria contexto)
    {
        return candidatos
            .Select(c =>
            {
                var jogoLogger = contexto.ObterJogoLogger(c.Jogo.IdJogo);
                var oferta = contexto.ObterOferta(c.Jogo.SteamAppId);
                var mediaFogueiras = ContextoCuradoria.CalcularMediaFogueiras(jogoLogger);
                var pontuacao = Math.Round(CalcularPontuacao(c.Jogo, c.Afinidade, contexto), 2);
                return new JogoCuradoResultado(c.Jogo, pontuacao, mediaFogueiras, oferta);
            })
            .OrderByDescending(r => r.PontuacaoCuradoria)
            .ThenByDescending(r => r.Jogo.NotaMetacritic ?? 0)
            .ThenBy(r => r.Jogo.Nome)
            .ToList();
    }
}

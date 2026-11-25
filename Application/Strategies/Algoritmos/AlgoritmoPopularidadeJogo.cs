using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;

namespace Application.Strategies.Algoritmos;

public class AlgoritmoPopularidadeJogo : AlgoritmoCuradoriaCatalogo
{
    public override TipoAlgoritmoCuradoria TipoAlgoritmo => TipoAlgoritmoCuradoria.Popularidade;
    public override string ChaveExibicao => "popularidade";

    public override double CalcularPontuacao(JogoCatalogoRespostDto jogo, double afinidadeBusca, ContextoCuradoria contexto)
    {
        var jogoLogger = contexto.ObterJogoLogger(jogo.IdJogo);
        var engajamentoProprioJogo = ContextoCuradoria.CalcularForcaJogoLogger(jogoLogger);
        var notaCritica = (jogo.NotaMetacritic ?? 65) * 0.35;
        var recomendacoesSteam = Math.Log10(Math.Max(jogo.TotalRecomendacoes, 0) + 1) * 5.0;
        var desempateGenero = contexto.CalcularAfinidadeGenerosDoJogo(jogo) * 0.10;

        return (afinidadeBusca * 30.0)
             + engajamentoProprioJogo
             + notaCritica
             + recomendacoesSteam
             + desempateGenero;
    }
}

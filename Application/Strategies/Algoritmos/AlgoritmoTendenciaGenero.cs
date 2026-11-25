using Domain.Model;

namespace Application.Strategies.Algoritmos;

public class AlgoritmoTendenciaGenero : AlgoritmoCuradoriaCatalogo
{
    public override string NomeAlgoritmo => "tendencia";

    public override double CalcularPontuacao(Jogo jogo, double afinidadeBusca, ContextoCuradoria contexto)
    {
        var afinidadeGenero = contexto.CalcularAfinidadeGenerosDoJogo(jogo);
        var jogoLogger = contexto.ObterJogoLogger(jogo.IdJogo);
        var engajamentoJogo = ContextoCuradoria.CalcularForcaJogoLogger(jogoLogger) * 0.20;
        var notaCritica = (jogo.NotaMetacritic ?? 60) * 0.20;

        return (afinidadeBusca * 25.0)
             + (afinidadeGenero * 2.5)
             + engajamentoJogo
             + notaCritica;
    }
}

using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;

namespace Application.Strategies.Algoritmos;

public class AlgoritmoTendenciaGenero : AlgoritmoCuradoriaCatalogo
{
    public override TipoAlgoritmoCuradoria TipoAlgoritmo => TipoAlgoritmoCuradoria.Tendencia;
    public override string ChaveExibicao => "tendencia";

    public override double CalcularPontuacao(JogoCatalogoRespostDto jogo, double afinidadeBusca, ContextoCuradoria contexto)
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

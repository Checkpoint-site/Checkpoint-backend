using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;

namespace Application.Strategies.Algoritmos;

public class AlgoritmoCustoBeneficio : AlgoritmoCuradoriaCatalogo
{
    public override TipoAlgoritmoCuradoria TipoAlgoritmo => TipoAlgoritmoCuradoria.CustoBeneficio;
    public override string ChaveExibicao => "custo-beneficio";

    public override double CalcularPontuacao(JogoCatalogoRespostDto jogo, double afinidadeBusca, ContextoCuradoria contexto)
    {
        var notaBase = (double)(jogo.NotaMetacritic ?? 70);
        var jogoLogger = contexto.ObterJogoLogger(jogo.IdJogo);
        var bonusFogueiras = ContextoCuradoria.CalcularMediaFogueiras(jogoLogger) * 4.0;
        var oferta = contexto.ObterOferta(jogo.SteamAppId);

        double fatorEconomia;
        if (jogo.Gratuito || oferta?.Gratuito == true || oferta?.PrecoFinalCentavos == 0)
        {
            fatorEconomia = 50.0;
        }
        else if (oferta?.PrecoFinalCentavos is > 0)
        {
            var precoReais = oferta.PrecoFinalCentavos.Value / 100.0;
            var razaoQualidadePreco = Math.Min(30.0, (notaBase / Math.Max(precoReais, 1.0)) * 4.0);
            fatorEconomia = (oferta.PercentualDesconto * 0.60) + razaoQualidadePreco;
        }
        else
        {
            fatorEconomia = (oferta?.PercentualDesconto ?? 0) * 0.50;
        }

        return (afinidadeBusca * 20.0)
             + fatorEconomia
             + (notaBase * 0.25)
             + bonusFogueiras;
    }
}

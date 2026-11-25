using Domain.Model;

namespace Application.Strategies.Busca;

public class BuscaPorAutorStrategy : EstrategiaBuscaCatalogo
{
    public override string TipoBusca => "autor";

    public override bool AtendeCriterio(Jogo jogo, string termo)
    {
        var filtro = termo.Trim();
        return jogo.Desenvolvedora.Contains(filtro, StringComparison.OrdinalIgnoreCase)
            || jogo.Publicadora.Contains(filtro, StringComparison.OrdinalIgnoreCase);
    }

    public override double CalcularAfinidade(Jogo jogo, string termo)
    {
        var filtro = termo.Trim();

        if (jogo.Desenvolvedora.Equals(filtro, StringComparison.OrdinalIgnoreCase))
        {
            return 1.0;
        }

        if (jogo.Desenvolvedora.Contains(filtro, StringComparison.OrdinalIgnoreCase))
        {
            return 0.9;
        }

        if (jogo.Publicadora.Equals(filtro, StringComparison.OrdinalIgnoreCase))
        {
            return 0.85;
        }

        return jogo.Publicadora.Contains(filtro, StringComparison.OrdinalIgnoreCase) ? 0.7 : 0.0;
    }
}

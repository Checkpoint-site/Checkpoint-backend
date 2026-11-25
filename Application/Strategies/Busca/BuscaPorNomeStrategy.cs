using Domain.Model;

namespace Application.Strategies.Busca;

public class BuscaPorNomeStrategy : EstrategiaBuscaCatalogo
{
    public override string TipoBusca => "nome";

    public override bool AtendeCriterio(Jogo jogo, string termo)
    {
        var filtro = termo.Trim();
        return jogo.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase);
    }

    public override double CalcularAfinidade(Jogo jogo, string termo)
    {
        var filtro = termo.Trim();

        if (jogo.Nome.Equals(filtro, StringComparison.OrdinalIgnoreCase))
        {
            return 1.0;
        }

        if (jogo.Nome.StartsWith(filtro, StringComparison.OrdinalIgnoreCase))
        {
            return 0.9;
        }

        return jogo.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase) ? 0.75 : 0.0;
    }
}

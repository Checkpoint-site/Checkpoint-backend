using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;

namespace Application.Strategies.Busca;

public class BuscaPorNomeStrategy : EstrategiaBuscaCatalogo
{
    public override TipoBuscaCatalogo TipoBusca => TipoBuscaCatalogo.Nome;

    public override bool AtendeCriterio(JogoCatalogoRespostDto jogo, string jogoBuscado)
    {
        var filtro = jogoBuscado.Trim();
        return jogo.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase);
    }

    public override double CalcularAfinidade(JogoCatalogoRespostDto jogo, string jogoBuscado)
    {
        var filtro = jogoBuscado.Trim();

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

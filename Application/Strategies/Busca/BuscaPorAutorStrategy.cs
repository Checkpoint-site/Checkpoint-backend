using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;

namespace Application.Strategies.Busca;

public class BuscaPorAutorStrategy : EstrategiaBuscaCatalogo
{
    public override TipoBuscaCatalogo TipoBusca => TipoBuscaCatalogo.Autor;

    public override bool AtendeCriterio(JogoCatalogoRespostDto jogo, string jogoBuscado)
    {
        var filtro = jogoBuscado.Trim();
        return jogo.Desenvolvedora.Contains(filtro, StringComparison.OrdinalIgnoreCase)
            || jogo.Publicadora.Contains(filtro, StringComparison.OrdinalIgnoreCase);
    }

    public override double CalcularAfinidade(JogoCatalogoRespostDto jogo, string jogoBuscado)
    {
        var filtro = jogoBuscado.Trim();

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

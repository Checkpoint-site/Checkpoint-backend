using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;

namespace Application.Strategies.Busca;

public class BuscaPorAutorStrategy : EstrategiaBuscaCatalogo
{
    public override TipoBuscaCatalogo TipoBusca => TipoBuscaCatalogo.Autor;

    public override bool AtendeCriterio(JogoCatalogoRespostDto jogo, string jogoBuscado)
    {
        var buscaNorm = NormalizarTextoBusca(jogoBuscado);
        var devNorm = NormalizarTextoBusca(jogo.Desenvolvedora);
        var pubNorm = NormalizarTextoBusca(jogo.Publicadora);

        return ContemTodosOsTermos(devNorm, buscaNorm)
            || ContemTodosOsTermos(pubNorm, buscaNorm);
    }

    public override double CalcularAfinidade(JogoCatalogoRespostDto jogo, string jogoBuscado)
    {
        var buscaNorm = NormalizarTextoBusca(jogoBuscado);
        var devNorm = NormalizarTextoBusca(jogo.Desenvolvedora);
        var pubNorm = NormalizarTextoBusca(jogo.Publicadora);

        if (devNorm.Equals(buscaNorm, StringComparison.Ordinal))
        {
            return 1.0;
        }

        if (ContemTodosOsTermos(devNorm, buscaNorm))
        {
            return 0.9;
        }

        if (pubNorm.Equals(buscaNorm, StringComparison.Ordinal))
        {
            return 0.85;
        }

        return ContemTodosOsTermos(pubNorm, buscaNorm) ? 0.7 : 0.0;
    }
}

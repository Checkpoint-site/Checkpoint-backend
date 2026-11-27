using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;

namespace Application.Strategies.Busca;

public class BuscaPorNomeStrategy : EstrategiaBuscaCatalogo
{
    public override TipoBuscaCatalogo TipoBusca => TipoBuscaCatalogo.Nome;

    public override bool AtendeCriterio(JogoCatalogoRespostDto jogo, string jogoBuscado)
    {
        var nomeNorm = NormalizarTextoBusca(jogo.Nome);
        var buscaNorm = NormalizarTextoBusca(jogoBuscado);
        return ContemTodosOsTermos(nomeNorm, buscaNorm);
    }

    public override double CalcularAfinidade(JogoCatalogoRespostDto jogo, string jogoBuscado)
    {
        var nomeNorm = NormalizarTextoBusca(jogo.Nome);
        var buscaNorm = NormalizarTextoBusca(jogoBuscado);

        if (string.IsNullOrEmpty(nomeNorm) || string.IsNullOrEmpty(buscaNorm))
        {
            return 0.0;
        }

        if (nomeNorm.Equals(buscaNorm, StringComparison.Ordinal))
        {
            return 1.0;
        }

        if (nomeNorm.StartsWith(buscaNorm, StringComparison.Ordinal))
        {
            return 0.9;
        }

        if (nomeNorm.Contains(buscaNorm, StringComparison.Ordinal))
        {
            return 0.75;
        }

        return ContemTodosOsTermos(nomeNorm, buscaNorm) ? 0.65 : 0.0;
    }
}

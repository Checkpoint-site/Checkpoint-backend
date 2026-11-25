using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Strategies.Busca;

public class BuscaPorAnoStrategy : EstrategiaBuscaCatalogo
{
    public override TipoBuscaCatalogo TipoBusca => TipoBuscaCatalogo.Ano;

    public override void ValidarBusca(string jogoBuscado)
    {
        if (string.IsNullOrWhiteSpace(jogoBuscado) || !int.TryParse(jogoBuscado.Trim(), out var ano))
        {
            throw new ValidationException("Para busca por ano, informe um ano numérico válido de 4 dígitos (ex.: 2015).");
        }

        var anoMaximo = DateTime.UtcNow.Year + 5;
        if (ano < 1970 || ano > anoMaximo)
        {
            throw new ValidationException($"O ano de lançamento deve estar entre 1970 e {anoMaximo}.");
        }
    }

    public override bool AtendeCriterio(JogoCatalogoRespostDto jogo, string jogoBuscado)
    {
        return int.TryParse(jogoBuscado.Trim(), out var ano) && jogo.AnoLancamento == ano;
    }

    public override double CalcularAfinidade(JogoCatalogoRespostDto jogo, string jogoBuscado)
    {
        return AtendeCriterio(jogo, jogoBuscado) ? 1.0 : 0.0;
    }
}

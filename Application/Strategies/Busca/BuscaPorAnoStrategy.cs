using Domain.Exceptions.Common;
using Domain.Model;

namespace Application.Strategies.Busca;

public class BuscaPorAnoStrategy : EstrategiaBuscaCatalogo
{
    public override string TipoBusca => "ano";

    public override void ValidarTermo(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo) || !int.TryParse(termo.Trim(), out var ano))
        {
            throw new ValidationException("Para busca por ano, informe um ano numérico válido de 4 dígitos (ex.: 2015).");
        }

        var anoMaximo = DateTime.UtcNow.Year + 5;
        if (ano < 1970 || ano > anoMaximo)
        {
            throw new ValidationException($"O ano de lançamento deve estar entre 1970 e {anoMaximo}.");
        }
    }

    public override bool AtendeCriterio(Jogo jogo, string termo)
    {
        return int.TryParse(termo.Trim(), out var ano) && jogo.AnoLancamento == ano;
    }

    public override double CalcularAfinidade(Jogo jogo, string termo)
    {
        return AtendeCriterio(jogo, termo) ? 1.0 : 0.0;
    }
}

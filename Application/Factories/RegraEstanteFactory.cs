using Application.Interfaces;
using Application.Strategies.Estantes;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Factories;

public class RegraEstanteFactory(IEnumerable<RegraEstanteCatalogo> regras) : IRegraEstanteFactory
{
    private readonly IReadOnlyDictionary<TipoEstante, RegraEstanteCatalogo> _regras =
        regras.ToDictionary(r => r.TipoEstante);

    public RegraEstanteCatalogo Criar(TipoEstante tipoEstante)
    {
        if (_regras.TryGetValue(tipoEstante, out var regra))
        {
            return regra;
        }

        throw new ValidationException($"Regra de estante '{tipoEstante}' não está registrada.");
    }

    public RegraEstanteCatalogo Criar(string? tipoEstante)
    {
        if (string.IsNullOrWhiteSpace(tipoEstante))
        {
            throw new ValidationException("O tipo de estante é obrigatório.");
        }

        var normalizado = tipoEstante.Trim().Replace("-", string.Empty).Replace("_", string.Empty);

        if (Enum.TryParse<TipoEstante>(normalizado, ignoreCase: true, out var estanteEnum)
            && Enum.IsDefined(estanteEnum))
        {
            return Criar(estanteEnum);
        }

        var disponiveis = string.Join(", ", _regras.Values.Select(r => r.ChaveExibicao));
        throw new ValidationException($"Tipo de estante '{tipoEstante}' inválido. Valores aceitos: {disponiveis}.");
    }
}

using Application.Interfaces;
using Application.Strategies.Busca;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Factories;

public class BuscaCatalogoStrategyFactory(IEnumerable<EstrategiaBuscaCatalogo> estrategias) : IBuscaCatalogoStrategyFactory
{
    private readonly IReadOnlyDictionary<TipoBuscaCatalogo, EstrategiaBuscaCatalogo> _estrategias =
        estrategias.ToDictionary(e => e.TipoBusca);

    public EstrategiaBuscaCatalogo Criar(TipoBuscaCatalogo tipo)
    {
        if (_estrategias.TryGetValue(tipo, out var estrategia))
        {
            return estrategia;
        }

        throw new ValidationException($"Estratégia de busca '{tipo}' não está registrada.");
    }

    public EstrategiaBuscaCatalogo Criar(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return Criar(TipoBuscaCatalogo.Nome);
        }

        var normalizado = tipo.Trim().Replace("-", string.Empty).Replace("_", string.Empty);

        if (Enum.TryParse<TipoBuscaCatalogo>(normalizado, ignoreCase: true, out var tipoEnum)
            && Enum.IsDefined(tipoEnum))
        {
            return Criar(tipoEnum);
        }

        var disponiveis = string.Join(", ", _estrategias.Values.Select(e => e.ChaveExibicao));
        throw new ValidationException($"Tipo de busca '{tipo}' inválido. Valores aceitos: {disponiveis}.");
    }
}

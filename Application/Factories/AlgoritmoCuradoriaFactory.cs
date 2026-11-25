using Application.Interfaces;
using Application.Strategies.Algoritmos;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Factories;

public class AlgoritmoCuradoriaFactory(IEnumerable<AlgoritmoCuradoriaCatalogo> algoritmos) : IAlgoritmoCuradoriaFactory
{
    private readonly IReadOnlyDictionary<TipoAlgoritmoCuradoria, AlgoritmoCuradoriaCatalogo> _algoritmos =
        algoritmos.ToDictionary(a => a.TipoAlgoritmo);

    public AlgoritmoCuradoriaCatalogo Criar(TipoAlgoritmoCuradoria algoritmo)
    {
        if (_algoritmos.TryGetValue(algoritmo, out var instancia))
        {
            return instancia;
        }

        throw new ValidationException($"Algoritmo de curadoria '{algoritmo}' não está registrado.");
    }

    public AlgoritmoCuradoriaCatalogo Criar(string? algoritmo)
    {
        if (string.IsNullOrWhiteSpace(algoritmo))
        {
            return Criar(TipoAlgoritmoCuradoria.Popularidade);
        }

        var normalizado = algoritmo.Trim().Replace("-", string.Empty).Replace("_", string.Empty);

        if (Enum.TryParse<TipoAlgoritmoCuradoria>(normalizado, ignoreCase: true, out var algoritmoEnum)
            && Enum.IsDefined(algoritmoEnum))
        {
            return Criar(algoritmoEnum);
        }

        var disponiveis = string.Join(", ", _algoritmos.Values.Select(a => a.ChaveExibicao));
        throw new ValidationException($"Algoritmo '{algoritmo}' inválido. Valores aceitos: {disponiveis}.");
    }
}

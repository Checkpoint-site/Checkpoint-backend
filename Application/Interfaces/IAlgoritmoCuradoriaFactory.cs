using Application.Strategies.Algoritmos;
using Domain.Enum;

namespace Application.Interfaces;

public interface IAlgoritmoCuradoriaFactory
{
    AlgoritmoCuradoriaCatalogo Criar(TipoAlgoritmoCuradoria algoritmo);
    AlgoritmoCuradoriaCatalogo Criar(string? algoritmo);
}

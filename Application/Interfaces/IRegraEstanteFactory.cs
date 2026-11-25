using Application.Strategies.Estantes;
using Domain.Enum;

namespace Application.Interfaces;

public interface IRegraEstanteFactory
{
    RegraEstanteCatalogo Criar(TipoEstante tipoEstante);
    RegraEstanteCatalogo Criar(string? tipoEstante);
}

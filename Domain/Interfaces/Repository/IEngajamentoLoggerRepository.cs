using Domain.Dtos.Catalogo;
using Domain.Dtos.Loggers.Commands;
using Domain.Dtos.Loggers.Respost;

namespace Domain.Interfaces.Repository;

public interface IEngajamentoLoggerRepository
{
    Task<IReadOnlyList<GeneroLoggerRespostDto>> ObterGeneroLoggersAsync(CancellationToken cancellationToken = default);
    Task<JogoLoggerRespostDto?> ObterJogoLoggerAsync(int idJogo, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<int, JogoLoggerRespostDto>> ObterJogoLoggersPorJogosAsync(IEnumerable<int> idsJogos, CancellationToken cancellationToken = default);
    Task AplicarImpactoEstanteAsync(ImpactoEngajamentoEstanteCommandDto command, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioPopularComunidadeDto>> ObterUsuariosPopularesAsync(int limite = 10, CancellationToken cancellationToken = default);
}

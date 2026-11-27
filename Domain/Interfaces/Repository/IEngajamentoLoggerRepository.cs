using Domain.Dtos.Catalogo;
using Domain.Dtos.Loggers.Commands;
using Domain.Dtos.Loggers.Respost;
using Domain.Enum;

namespace Domain.Interfaces.Repository;

public interface IEngajamentoLoggerRepository
{
    Task<IReadOnlyList<GeneroLoggerRespostDto>> ObterGeneroLoggersAsync(CancellationToken cancellationToken = default);
    Task<JogoLoggerRespostDto?> ObterJogoLoggerAsync(int idJogo, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<int, JogoLoggerRespostDto>> ObterJogoLoggersPorJogosAsync(IEnumerable<int> idsJogos, CancellationToken cancellationToken = default);
    Task AplicarImpactoEstanteAsync(ImpactoEngajamentoEstanteCommandDto command, CancellationToken cancellationToken = default);
    Task SincronizarAvaliacoesJogoEUsuarioAsync(int idJogo, int idUsuario, IReadOnlyList<Generos> generosJogo, int deltaPostsGenero = 0, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioPopularComunidadeDto>> ObterUsuariosPopularesAsync(int limite = 10, CancellationToken cancellationToken = default);
}

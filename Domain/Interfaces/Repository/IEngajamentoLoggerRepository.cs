using Domain.Dtos.Catalogo;
using Domain.Enum;
using Domain.Model;

namespace Domain.Interfaces.Repository;

public interface IEngajamentoLoggerRepository
{
    Task<IReadOnlyList<GeneroLogger>> ObterGeneroLoggersGlobaisAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneroLogger>> ObterGeneroLoggersPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<GeneroLogger> ObterOuCriarGeneroLoggerAsync(Generos genero, int? idUsuario, CancellationToken cancellationToken = default);
    Task<JogoLogger?> ObterJogoLoggerAsync(int idJogo, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<int, JogoLogger>> ObterJogoLoggersPorJogosAsync(IEnumerable<int> idsJogos, CancellationToken cancellationToken = default);
    Task<JogoLogger> ObterOuCriarJogoLoggerAsync(int idJogo, CancellationToken cancellationToken = default);
    Task<UsuarioLogger> ObterOuCriarUsuarioLoggerAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioPopularComunidadeDto>> ObterUsuariosPopularesAsync(int limite = 10, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}

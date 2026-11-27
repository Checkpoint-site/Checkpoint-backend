using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Commands;
using Domain.Dtos.Catalogo.Respost;

namespace Domain.Interfaces.Repository;

public interface ICatalogoRepository
{
    Task<JogoCatalogoRespostDto?> ObterPorIdAsync(int idJogo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JogoCatalogoRespostDto>> ObterPorIdsAsync(IEnumerable<int> idsJogos, CancellationToken cancellationToken = default);
    Task<JogoCatalogoRespostDto?> ObterPorSteamAppIdAsync(int steamAppId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JogoCatalogoRespostDto>> ObterPorSteamAppIdsAsync(IEnumerable<int> steamAppIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JogoCatalogoRespostDto>> BuscarCandidatosLocaisAsync(BuscarCandidatosLocaisCommandDto command, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JogoCatalogoRespostDto>> ListarCatalogoLocalAsync(int limite = 60, CancellationToken cancellationToken = default);
    Task<JogoCatalogoRespostDto> SalvarOuAtualizarAsync(DetalhesJogoSteamDto detalhesSteam, CancellationToken cancellationToken = default);
}

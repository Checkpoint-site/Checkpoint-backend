using Domain.Dtos.Catalogo;
using Domain.Model;

namespace Domain.Interfaces.Repository;

public interface ICatalogoRepository
{
    Task<Jogo?> ObterPorIdAsync(int idJogo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Jogo>> ObterPorIdsAsync(IEnumerable<int> idsJogos, CancellationToken cancellationToken = default);
    Task<Jogo?> ObterPorSteamAppIdAsync(int steamAppId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Jogo>> ObterPorSteamAppIdsAsync(IEnumerable<int> steamAppIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Jogo>> BuscarCandidatosLocaisAsync(string tipoBusca, string termo, CancellationToken cancellationToken = default);
    Task<Jogo> SalvarOuAtualizarAsync(DetalhesJogoSteamDto detalhesSteam, CancellationToken cancellationToken = default);
}

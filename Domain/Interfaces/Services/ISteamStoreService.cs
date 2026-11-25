using Domain.Dtos.Catalogo;

namespace Domain.Interfaces.Services;

public interface ISteamStoreService
{
    Task<IReadOnlyList<ItemBuscaSteamDto>> BuscarJogosAsync(string jogoBuscado, CancellationToken cancellationToken = default);
    Task<DestaquesSteamDto> ObterDestaquesAsync(CancellationToken cancellationToken = default);
    Task<DetalhesJogoSteamDto?> ObterDetalhesJogoAsync(int steamAppId, CancellationToken cancellationToken = default);
}

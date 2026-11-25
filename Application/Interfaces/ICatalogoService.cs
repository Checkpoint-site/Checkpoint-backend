using Application.Dtos.Catalogo.Responses;

namespace Application.Interfaces;

public interface ICatalogoService
{
    Task<BuscaCatalogoResponseDto> BuscarAsync(
        string? tipo,
        string termo,
        string? algoritmo,
        int? idUsuario = null,
        CancellationToken cancellationToken = default);

    Task<DestaquesCatalogoResponseDto> ObterDestaquesAsync(
        string? algoritmo = null,
        int? idUsuario = null,
        CancellationToken cancellationToken = default);

    Task<JogoDetalhesResponseDto> ObterDetalhesPorSteamAppIdAsync(
        int steamAppId,
        CancellationToken cancellationToken = default);

    Task<TendenciasGenerosResponseDto> ObterTendenciasGenerosAsync(
        int? idUsuario = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UsuarioPopularResponseDto>> ObterUsuariosPopularesAsync(
        int limite = 10,
        CancellationToken cancellationToken = default);
}

using Application.Dtos.Catalogo.Requests;
using Application.Dtos.Catalogo.Responses;

namespace Application.Interfaces;

public interface ICatalogoService
{
    Task<BuscaCatalogoResponseDto> BuscarAsync(
        BuscarCatalogoRequestDto request,
        CancellationToken cancellationToken = default);

    Task<DestaquesCatalogoResponseDto> ObterDestaquesAsync(
        ObterDestaquesCatalogoRequestDto request,
        CancellationToken cancellationToken = default);

    Task<JogoDetalhesResponseDto> ObterDetalhesPorSteamAppIdAsync(
        ObterDetalhesJogoRequestDto request,
        CancellationToken cancellationToken = default);

    Task<TendenciasGenerosResponseDto> ObterTendenciasGenerosAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UsuarioPopularResponseDto>> ObterUsuariosPopularesAsync(
        ObterUsuariosPopularesRequestDto request,
        CancellationToken cancellationToken = default);

    Task<DescobertaCatalogoResponseDto> ObterJogosAleatoriosAsync(
        ObterJogosAleatoriosRequestDto request,
        CancellationToken cancellationToken = default);
}

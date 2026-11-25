using Application.Dtos.Estantes.Requests;
using Application.Dtos.Estantes.Responses;

namespace Application.Interfaces;

public interface IEstanteService
{
    Task<EstantesUsuarioResponseDto> ObterEstantesUsuarioAsync(
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task<EstantesUsuarioResponseDto> AdicionarJogoAsync(
        int idUsuario,
        string tipoEstante,
        int steamAppId,
        CancellationToken cancellationToken = default);

    Task<EstantesUsuarioResponseDto> RemoverJogoAsync(
        int idUsuario,
        string tipoEstante,
        int steamAppId,
        CancellationToken cancellationToken = default);

    Task<EstanteFavoritosResponseDto> AtualizarConfiguracaoFavoritosAsync(
        int idUsuario,
        AtualizarFavoritosConfigRequestDto request,
        CancellationToken cancellationToken = default);

    Task<EstantePretendeJogarResponseDto> AtualizarConfiguracaoPretendeJogarAsync(
        int idUsuario,
        AtualizarPretendeJogarConfigRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PromocoesPretendeJogarResponseDto> ObterPromocoesPretendeJogarAsync(
        int idUsuario,
        CancellationToken cancellationToken = default);
}

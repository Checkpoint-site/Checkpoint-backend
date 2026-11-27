using Application.Dtos.Vitrine.Responses;

namespace Application.Interfaces;

public interface IVitrineService
{
    Task<VitrineHomeResponseDto> ObterVitrineHomeAsync(
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default);

    Task<VitrineHomeResponseDto> ObterVitrineFallbackAsync(
        int limite = 24,
        string? algoritmo = null,
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default);
}

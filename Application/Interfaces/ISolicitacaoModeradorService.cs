using Application.Dtos.Moderacao.Requests;
using Application.Dtos.Moderacao.Responses;

namespace Application.Interfaces;

public interface ISolicitacaoModeradorService
{
    Task<SolicitacaoModeradorResponseDto> SolicitarAsync(int idUsuarioSolicitante, CriarSolicitacaoModeradorRequestDto request);
    Task<SolicitacaoModeradorResponseDto?> ObterMinhaSolicitacaoAsync(int idUsuarioSolicitante);
    Task<IReadOnlyList<SolicitacaoModeradorResponseDto>> ListarAsync(string? status = null);
    Task<SolicitacaoModeradorResponseDto> AprovarAsync(int idSolicitacao, int idAvaliador);
    Task<SolicitacaoModeradorResponseDto> RecusarAsync(int idSolicitacao, int idAvaliador, RecusarSolicitacaoModeradorRequestDto request);
    Task<SolicitacaoModeradorResponseDto> CancelarAsync(int idSolicitacao, int idUsuarioSolicitante);
    Task RebaixarModeradorAsync(int idAdmin, int idUsuarioAlvo);
    Task<PostSpoilerResponseDto> AlterarSpoilerPostAsync(int idPost, int idAvaliador, AlterarSpoilerPostRequestDto request);
}

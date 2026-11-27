using Application.Dtos.Posts.Requests;
using Application.Dtos.Posts.Responses;

namespace Application.Interfaces;

public interface IPostService
{
    Task<IReadOnlyList<PostResponseDto>> ListarRecentesAsync(int limite = 20, int? idUsuarioAtual = null, CancellationToken cancellationToken = default);
    Task<PostComComentariosResponseDto> ObterDetalhesComComentariosAsync(int idPost, int? idUsuarioAtual = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PostResponseDto>> ListarPorJogoSteamAppIdAsync(int steamAppId, string? ordenacao = null, int? idUsuarioAtual = null, CancellationToken cancellationToken = default);
    Task<PostResponseDto> SalvarPorJogoSteamAppIdAsync(int idUsuario, int steamAppId, SalvarPostRequestDto request, CancellationToken cancellationToken = default);
    Task ExcluirAsync(int idPost, int idUsuarioSolicitante, string? roleSolicitante = null, CancellationToken cancellationToken = default);
    Task<CurtidaResponseDto> AlternarCurtidaPostAsync(int idPost, int idUsuario, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ComentarioResponseDto>> ListarComentariosAsync(int idPost, int? idUsuarioAtual = null, CancellationToken cancellationToken = default);
    Task<ComentarioResponseDto> AdicionarComentarioAsync(int idPost, int idUsuario, CriarComentarioRequestDto request, CancellationToken cancellationToken = default);
    Task<CurtidaResponseDto> AlternarCurtidaComentarioAsync(int idComentario, int idUsuario, CancellationToken cancellationToken = default);
}

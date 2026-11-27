using Domain.Dtos.Posts.Commands;
using Domain.Dtos.Posts.Respost;

namespace Domain.Interfaces.Repository;

public interface IPostRepository
{
    Task<PostDetalheRespostDto?> ObterPorIdAsync(int idPost, int? idUsuarioAtual = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PostDetalheRespostDto>> ListarRecentesAsync(int limite = 20, int? idUsuarioAtual = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PostDetalheRespostDto>> ListarPorJogoAsync(int idJogo, string? ordenacao = null, int? idUsuarioAtual = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PostDetalheRespostDto>> ListarPorUsuarioAsync(int idUsuario, int? idUsuarioAtual = null, CancellationToken cancellationToken = default);
    Task<(PostDetalheRespostDto Post, bool FoiCriadoNovo)> SalvarOuAtualizarDoUsuarioNoJogoAsync(SalvarPostCommandDto command, CancellationToken cancellationToken = default);
    Task<bool> ExcluirAsync(int idPost, int idUsuarioSolicitante, bool podeModerar = false, CancellationToken cancellationToken = default);
    Task<ResultadoCurtidaRespostDto> AlternarCurtidaPostAsync(int idPost, int idUsuario, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ComentarioRespostDto>> ListarComentariosPorPostAsync(int idPost, int? idUsuarioAtual = null, CancellationToken cancellationToken = default);
    Task<ComentarioRespostDto> AdicionarComentarioAsync(CriarComentarioCommandDto command, CancellationToken cancellationToken = default);
    Task<ResultadoCurtidaRespostDto> AlternarCurtidaComentarioAsync(int idComentario, int idUsuario, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FaixaDistribuicaoFogueirasDto>> ObterDistribuicaoFogueirasJogoAsync(int idJogo, CancellationToken cancellationToken = default);
}

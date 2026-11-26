using Domain.Enum;
using Domain.Model;

namespace Domain.Interfaces.Repository;

public interface ISolicitacaoModeradorRepository
{
    Task<Usuario?> ObterUsuarioPorIdAsync(int idUsuario);
    Task<Post?> ObterPostPorIdAsync(int idPost);
    Task<bool> ExisteSolicitacaoAbertaAsync(int idUsuarioSolicitante);
    Task<SolicitacaoModerador?> ObterPorIdAsync(int idSolicitacao);
    Task<SolicitacaoModerador?> ObterUltimaPorUsuarioAsync(int idUsuarioSolicitante);
    Task<IReadOnlyList<SolicitacaoModerador>> ListarAsync(StatusSolicitacaoModerador? status = null);
    Task<SolicitacaoModerador> CriarAsync(SolicitacaoModerador solicitacao);
    Task SalvarAlteracoesAsync();
}

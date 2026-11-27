using Domain.Dtos.Diario.Commands;
using Domain.Dtos.Diario.Respost;

namespace Domain.Interfaces.Repository;

public interface IDiarioRepository
{
    Task<IReadOnlyList<RegistroDiarioRespostDto>> ListarPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<RegistroDiarioRespostDto?> ObterPorUsuarioEJogoAsync(int idUsuario, int idJogo, CancellationToken cancellationToken = default);
    Task<RegistroDiarioRespostDto> SalvarOuAtualizarAsync(SalvarAvaliacaoDiarioCommandDto command, CancellationToken cancellationToken = default);
}

using Application.Dtos.Diario.Requests;
using Application.Dtos.Diario.Responses;

namespace Application.Interfaces;

public interface IDiarioService
{
    Task<BibliotecaUsuarioResponseDto> ObterMinhaBibliotecaAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<AvaliacaoSalvaResponseDto> SalvarAvaliacaoJogoAsync(int idUsuario, int steamAppId, SalvarAvaliacaoJogoRequestDto request, CancellationToken cancellationToken = default);
    Task<RegistroJogoUsuarioResponseDto> AlternarJogadoAsync(int idUsuario, int steamAppId, CancellationToken cancellationToken = default);
}

using Domain.Dtos.Estantes.Commands;
using Domain.Dtos.Estantes.Respost;

namespace Domain.Interfaces.Repository;

public interface IEstanteRepository
{
    Task<FavoritosRespostDto?> ObterFavoritosPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<PretendeJogarRespostDto?> ObterPretendeJogarPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<FavoritosRespostDto> SalvarFavoritosAsync(SalvarFavoritosCommandDto command, CancellationToken cancellationToken = default);
    Task<PretendeJogarRespostDto> SalvarPretendeJogarAsync(SalvarPretendeJogarCommandDto command, CancellationToken cancellationToken = default);
}

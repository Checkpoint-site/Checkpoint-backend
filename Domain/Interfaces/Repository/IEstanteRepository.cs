using Domain.Model;

namespace Domain.Interfaces.Repository;

public interface IEstanteRepository
{
    Task<Favoritos?> ObterFavoritosPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<PretendeJogar?> ObterPretendeJogarPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<Favoritos> SalvarFavoritosAsync(Favoritos favoritos, CancellationToken cancellationToken = default);
    Task<PretendeJogar> SalvarPretendeJogarAsync(PretendeJogar pretendeJogar, CancellationToken cancellationToken = default);
}

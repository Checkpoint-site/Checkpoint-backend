using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class EstanteRepository(ApplicationDbContext context) : IEstanteRepository
{
    public async Task<Favoritos?> ObterFavoritosPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        return await context.Favoritos
            .FirstOrDefaultAsync(f => f.IdUsuario == idUsuario, cancellationToken);
    }

    public async Task<PretendeJogar?> ObterPretendeJogarPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        return await context.PretendeJogar
            .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario, cancellationToken);
    }

    public async Task<Favoritos> SalvarFavoritosAsync(Favoritos favoritos, CancellationToken cancellationToken = default)
    {
        favoritos.DataAtualizacao = DateTime.UtcNow;

        if (favoritos.IdCatalogo == 0)
        {
            await context.Favoritos.AddAsync(favoritos, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        return favoritos;
    }

    public async Task<PretendeJogar> SalvarPretendeJogarAsync(PretendeJogar pretendeJogar, CancellationToken cancellationToken = default)
    {
        pretendeJogar.DataAtualizacao = DateTime.UtcNow;

        if (pretendeJogar.IdCatalogo == 0)
        {
            await context.PretendeJogar.AddAsync(pretendeJogar, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        return pretendeJogar;
    }
}

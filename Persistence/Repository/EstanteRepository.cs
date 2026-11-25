using Domain.Dtos.Estantes.Commands;
using Domain.Dtos.Estantes.Respost;
using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class EstanteRepository(ApplicationDbContext context) : IEstanteRepository
{
    public async Task<FavoritosRespostDto?> ObterFavoritosPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        return await context.Favoritos
            .AsNoTracking()
            .Where(f => f.IdUsuario == idUsuario)
            .Select(f => new FavoritosRespostDto
            {
                IdCatalogo = f.IdCatalogo,
                IdUsuario = f.IdUsuario,
                Jogos = f.Jogos,
                JogoDestaquePerfilId = f.JogoDestaquePerfilId,
                ExibirNoTopoPerfil = f.ExibirNoTopoPerfil,
                ObservacaoDestaque = f.ObservacaoDestaque,
                DataAtualizacao = f.DataAtualizacao
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PretendeJogarRespostDto?> ObterPretendeJogarPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        return await context.PretendeJogar
            .AsNoTracking()
            .Where(p => p.IdUsuario == idUsuario)
            .Select(p => new PretendeJogarRespostDto
            {
                IdCatalogo = p.IdCatalogo,
                IdUsuario = p.IdUsuario,
                Jogos = p.Jogos,
                NivelPrioridade = p.NivelPrioridade,
                NotificarEmPromocao = p.NotificarEmPromocao,
                MetaAnoConclusao = p.MetaAnoConclusao,
                DataAtualizacao = p.DataAtualizacao
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FavoritosRespostDto> SalvarFavoritosAsync(SalvarFavoritosCommandDto command, CancellationToken cancellationToken = default)
    {
        var existente = await context.Favoritos
            .FirstOrDefaultAsync(f => f.IdUsuario == command.IdUsuario, cancellationToken);

        if (existente is null)
        {
            existente = new Favoritos
            {
                IdUsuario = command.IdUsuario,
                Jogos = command.Jogos.ToList(),
                JogoDestaquePerfilId = command.JogoDestaquePerfilId,
                ExibirNoTopoPerfil = command.ExibirNoTopoPerfil,
                ObservacaoDestaque = command.ObservacaoDestaque,
                DataAtualizacao = DateTime.UtcNow
            };

            await context.Favoritos.AddAsync(existente, cancellationToken);
        }
        else
        {
            existente.Jogos = command.Jogos.ToList();
            existente.JogoDestaquePerfilId = command.JogoDestaquePerfilId;
            existente.ExibirNoTopoPerfil = command.ExibirNoTopoPerfil;
            existente.ObservacaoDestaque = command.ObservacaoDestaque;
            existente.DataAtualizacao = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new FavoritosRespostDto
        {
            IdCatalogo = existente.IdCatalogo,
            IdUsuario = existente.IdUsuario,
            Jogos = existente.Jogos.ToList(),
            JogoDestaquePerfilId = existente.JogoDestaquePerfilId,
            ExibirNoTopoPerfil = existente.ExibirNoTopoPerfil,
            ObservacaoDestaque = existente.ObservacaoDestaque,
            DataAtualizacao = existente.DataAtualizacao
        };
    }

    public async Task<PretendeJogarRespostDto> SalvarPretendeJogarAsync(SalvarPretendeJogarCommandDto command, CancellationToken cancellationToken = default)
    {
        var existente = await context.PretendeJogar
            .FirstOrDefaultAsync(p => p.IdUsuario == command.IdUsuario, cancellationToken);

        if (existente is null)
        {
            existente = new PretendeJogar
            {
                IdUsuario = command.IdUsuario,
                Jogos = command.Jogos.ToList(),
                NivelPrioridade = command.NivelPrioridade,
                NotificarEmPromocao = command.NotificarEmPromocao,
                MetaAnoConclusao = command.MetaAnoConclusao,
                DataAtualizacao = DateTime.UtcNow
            };

            await context.PretendeJogar.AddAsync(existente, cancellationToken);
        }
        else
        {
            existente.Jogos = command.Jogos.ToList();
            existente.NivelPrioridade = command.NivelPrioridade;
            existente.NotificarEmPromocao = command.NotificarEmPromocao;
            existente.MetaAnoConclusao = command.MetaAnoConclusao;
            existente.DataAtualizacao = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new PretendeJogarRespostDto
        {
            IdCatalogo = existente.IdCatalogo,
            IdUsuario = existente.IdUsuario,
            Jogos = existente.Jogos.ToList(),
            NivelPrioridade = existente.NivelPrioridade,
            NotificarEmPromocao = existente.NotificarEmPromocao,
            MetaAnoConclusao = existente.MetaAnoConclusao,
            DataAtualizacao = existente.DataAtualizacao
        };
    }
}

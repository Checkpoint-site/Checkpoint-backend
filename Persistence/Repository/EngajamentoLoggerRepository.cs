using Domain.Dtos.Catalogo;
using Domain.Enum;
using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class EngajamentoLoggerRepository(ApplicationDbContext context) : IEngajamentoLoggerRepository
{
    public async Task<IReadOnlyList<GeneroLogger>> ObterGeneroLoggersGlobaisAsync(CancellationToken cancellationToken = default)
    {
        return await context.GeneroLoggers
            .AsNoTracking()
            .Where(g => g.IdUsuario == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GeneroLogger>> ObterGeneroLoggersPorUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        return await context.GeneroLoggers
            .AsNoTracking()
            .Where(g => g.IdUsuario == idUsuario)
            .ToListAsync(cancellationToken);
    }

    public async Task<GeneroLogger> ObterOuCriarGeneroLoggerAsync(Generos genero, int? idUsuario, CancellationToken cancellationToken = default)
    {
        var local = context.GeneroLoggers.Local
            .FirstOrDefault(g => g.Genero == genero && g.IdUsuario == idUsuario);

        if (local is not null)
        {
            return local;
        }

        var existente = await context.GeneroLoggers
            .FirstOrDefaultAsync(g => g.Genero == genero && g.IdUsuario == idUsuario, cancellationToken);

        if (existente is not null)
        {
            return existente;
        }

        var novo = new GeneroLogger
        {
            Genero = genero,
            IdUsuario = idUsuario,
            UltimaAtualizacao = DateTime.UtcNow
        };

        await context.GeneroLoggers.AddAsync(novo, cancellationToken);
        return novo;
    }

    public async Task<JogoLogger?> ObterJogoLoggerAsync(int idJogo, CancellationToken cancellationToken = default)
    {
        return await context.JogoLoggers
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.IdJogo == idJogo, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, JogoLogger>> ObterJogoLoggersPorJogosAsync(IEnumerable<int> idsJogos, CancellationToken cancellationToken = default)
    {
        var listaIds = idsJogos.Distinct().ToList();
        if (listaIds.Count == 0)
        {
            return new Dictionary<int, JogoLogger>();
        }

        return await context.JogoLoggers
            .AsNoTracking()
            .Where(j => listaIds.Contains(j.IdJogo))
            .ToDictionaryAsync(j => j.IdJogo, cancellationToken);
    }

    public async Task<JogoLogger> ObterOuCriarJogoLoggerAsync(int idJogo, CancellationToken cancellationToken = default)
    {
        var local = context.JogoLoggers.Local
            .FirstOrDefault(j => j.IdJogo == idJogo);

        if (local is not null)
        {
            return local;
        }

        var existente = await context.JogoLoggers
            .FirstOrDefaultAsync(j => j.IdJogo == idJogo, cancellationToken);

        if (existente is not null)
        {
            return existente;
        }

        var novo = new JogoLogger
        {
            IdJogo = idJogo,
            UltimaAtualizacao = DateTime.UtcNow
        };

        await context.JogoLoggers.AddAsync(novo, cancellationToken);
        return novo;
    }

    public async Task<UsuarioLogger> ObterOuCriarUsuarioLoggerAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        var local = context.UsuarioLoggers.Local
            .FirstOrDefault(u => u.IdUsuario == idUsuario);

        if (local is not null)
        {
            return local;
        }

        var existente = await context.UsuarioLoggers
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, cancellationToken);

        if (existente is not null)
        {
            return existente;
        }

        var novo = new UsuarioLogger
        {
            IdUsuario = idUsuario,
            UltimaAtualizacao = DateTime.UtcNow
        };

        await context.UsuarioLoggers.AddAsync(novo, cancellationToken);
        return novo;
    }

    public async Task<IReadOnlyList<UsuarioPopularComunidadeDto>> ObterUsuariosPopularesAsync(int limite = 10, CancellationToken cancellationToken = default)
    {
        var quantidade = Math.Clamp(limite, 1, 50);

        var loggers = await (
            from logger in context.UsuarioLoggers.AsNoTracking()
            join usuario in context.Usuarios.AsNoTracking() on logger.IdUsuario equals usuario.Id
            where !usuario.ContaDeletada
            select new
            {
                Logger = logger,
                usuario.Nome
            })
            .ToListAsync(cancellationToken);

        var topUsuarios = loggers
            .OrderByDescending(x =>
                (x.Logger.CurtidasRecebidas * 4UL) +
                (x.Logger.PresencaPost * 5UL) +
                (x.Logger.QuantidadeSeguidores * 3UL) +
                (x.Logger.PresencaComentario * 2UL) +
                x.Logger.PresencaFavoritos)
            .Take(quantidade)
            .ToList();

        if (topUsuarios.Count == 0)
        {
            return [];
        }

        var idsUsuarios = topUsuarios.Select(u => u.Logger.IdUsuario).ToList();

        var relatos = await (
            from post in context.Posts.AsNoTracking()
            join jogo in context.Jogos.AsNoTracking() on post.IdJogo equals jogo.IdJogo
            where idsUsuarios.Contains(post.IdUsuario) && !post.PostDeletado
            orderby post.DataCriacao descending
            select new
            {
                post.IdUsuario,
                Relato = new RelatoResumoComunidadeDto
                {
                    IdPost = post.IdPost,
                    IdJogo = post.IdJogo,
                    NomeJogo = jogo.Nome,
                    Titulo = post.Titulo,
                    Fogueiras = post.Fogueiras,
                    QuantidadeCurtidas = post.QuantidadeCurtidas,
                    DataCriacao = post.DataCriacao
                }
            })
            .ToListAsync(cancellationToken);

        var relatosPorUsuario = relatos
            .GroupBy(r => r.IdUsuario)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<RelatoResumoComunidadeDto>)g.Select(x => x.Relato).Take(3).ToList());

        return topUsuarios
            .Select(u => new UsuarioPopularComunidadeDto
            {
                IdUsuario = u.Logger.IdUsuario,
                NomeUsuario = u.Nome,
                PresencaPost = u.Logger.PresencaPost,
                PresencaComentario = u.Logger.PresencaComentario,
                PresencaFavoritos = u.Logger.PresencaFavoritos,
                QuantidadeCurtida = u.Logger.QuantidadeCurtida,
                CurtidasRecebidas = u.Logger.CurtidasRecebidas,
                QuantidadeSeguidores = u.Logger.QuantidadeSeguidores,
                UltimosRelatos = relatosPorUsuario.TryGetValue(u.Logger.IdUsuario, out var ultimos)
                    ? ultimos
                    : []
            })
            .ToList();
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}

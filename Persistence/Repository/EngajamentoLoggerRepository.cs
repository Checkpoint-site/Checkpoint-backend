using Domain.Dtos.Catalogo;
using Domain.Dtos.Loggers.Commands;
using Domain.Dtos.Loggers.Respost;
using Domain.Enum;
using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class EngajamentoLoggerRepository(ApplicationDbContext context) : IEngajamentoLoggerRepository
{
    public async Task<IReadOnlyList<GeneroLoggerRespostDto>> ObterGeneroLoggersAsync(CancellationToken cancellationToken = default)
    {
        return await context.GeneroLoggers
            .AsNoTracking()
            .Select(g => new GeneroLoggerRespostDto
            {
                IdLogger = g.IdLogger,
                Genero = g.Genero,
                QuantidadeCurtida = g.QuantidadeCurtida,
                PresencaPost = g.PresencaPost,
                PresencaComentario = g.PresencaComentario,
                PresencaFavoritos = g.PresencaFavoritos,
                PresencaPretendeJogar = g.PresencaPretendeJogar,
                UltimaAtualizacao = g.UltimaAtualizacao
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<JogoLoggerRespostDto?> ObterJogoLoggerAsync(int idJogo, CancellationToken cancellationToken = default)
    {
        return await context.JogoLoggers
            .AsNoTracking()
            .Where(j => j.IdJogo == idJogo)
            .Select(j => new JogoLoggerRespostDto
            {
                IdLogger = j.IdLogger,
                IdJogo = j.IdJogo,
                QuantidadeCurtida = j.QuantidadeCurtida,
                PresencaPost = j.PresencaPost,
                PresencaComentario = j.PresencaComentario,
                PresencaFavoritos = j.PresencaFavoritos,
                PresencaPretendeJogar = j.PresencaPretendeJogar,
                SomaFogueiras = j.SomaFogueiras,
                UltimaAtualizacao = j.UltimaAtualizacao
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, JogoLoggerRespostDto>> ObterJogoLoggersPorJogosAsync(IEnumerable<int> idsJogos, CancellationToken cancellationToken = default)
    {
        var listaIds = idsJogos.Distinct().ToList();
        if (listaIds.Count == 0)
        {
            return new Dictionary<int, JogoLoggerRespostDto>();
        }

        return await context.JogoLoggers
            .AsNoTracking()
            .Where(j => listaIds.Contains(j.IdJogo))
            .Select(j => new JogoLoggerRespostDto
            {
                IdLogger = j.IdLogger,
                IdJogo = j.IdJogo,
                QuantidadeCurtida = j.QuantidadeCurtida,
                PresencaPost = j.PresencaPost,
                PresencaComentario = j.PresencaComentario,
                PresencaFavoritos = j.PresencaFavoritos,
                PresencaPretendeJogar = j.PresencaPretendeJogar,
                SomaFogueiras = j.SomaFogueiras,
                UltimaAtualizacao = j.UltimaAtualizacao
            })
            .ToDictionaryAsync(j => j.IdJogo, cancellationToken);
    }

    public async Task AplicarImpactoEstanteAsync(ImpactoEngajamentoEstanteCommandDto command, CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;

        var jogoLogger = await ObterOuCriarJogoLoggerInternoAsync(command.IdJogo, cancellationToken);
        jogoLogger.PresencaFavoritos = AplicarDelta(jogoLogger.PresencaFavoritos, command.DeltaFavoritosJogo);
        jogoLogger.PresencaPretendeJogar = AplicarDelta(jogoLogger.PresencaPretendeJogar, command.DeltaPretendeJogarJogo);
        jogoLogger.UltimaAtualizacao = agora;

        var usuarioLogger = await ObterOuCriarUsuarioLoggerInternoAsync(command.IdUsuario, cancellationToken);
        usuarioLogger.PresencaFavoritos = AplicarDelta(usuarioLogger.PresencaFavoritos, command.DeltaFavoritosUsuario);
        usuarioLogger.UltimaAtualizacao = agora;

        foreach (var genero in command.Generos.Distinct())
        {
            var generoLogger = await ObterOuCriarGeneroLoggerInternoAsync(genero, cancellationToken);
            generoLogger.PresencaFavoritos = AplicarDelta(generoLogger.PresencaFavoritos, command.DeltaFavoritosGenero);
            generoLogger.PresencaPretendeJogar = AplicarDelta(generoLogger.PresencaPretendeJogar, command.DeltaPretendeJogarGenero);
            generoLogger.UltimaAtualizacao = agora;
        }

        await context.SaveChangesAsync(cancellationToken);
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
                usuario.Nome,
                SeguidoresUsuario = usuario.QuantidadeSeguidores,
                PossuiFotoPerfil = usuario.FotoPerfil != null && usuario.FotoPerfil != string.Empty
            })
            .ToListAsync(cancellationToken);

        var topUsuarios = loggers
            .Select(x =>
            {
                var seguidores = Math.Max(
                    x.Logger.QuantidadeSeguidores,
                    x.SeguidoresUsuario > 0 ? (ulong)x.SeguidoresUsuario : 0UL);

                var pontuacao = (x.Logger.CurtidasRecebidas * 4UL) +
                                (x.Logger.PresencaPost * 5UL) +
                                (seguidores * 3UL) +
                                (x.Logger.PresencaComentario * 2UL) +
                                x.Logger.PresencaFavoritos +
                                (x.PossuiFotoPerfil ? 10UL : 0UL);

                return new
                {
                    x.Logger,
                    x.Nome,
                    QuantidadeSeguidores = seguidores,
                    x.PossuiFotoPerfil,
                    Pontuacao = pontuacao
                };
            })
            .OrderByDescending(x => x.Pontuacao)
            .ThenBy(x => x.Nome)
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
                QuantidadeSeguidores = u.QuantidadeSeguidores,
                PossuiFotoPerfil = u.PossuiFotoPerfil,
                UltimosRelatos = relatosPorUsuario.TryGetValue(u.Logger.IdUsuario, out var ultimos)
                    ? ultimos
                    : []
            })
            .ToList();
    }

    private async Task<GeneroLogger> ObterOuCriarGeneroLoggerInternoAsync(Generos genero, CancellationToken cancellationToken)
    {
        var local = context.GeneroLoggers.Local.FirstOrDefault(g => g.Genero == genero);
        if (local is not null)
        {
            return local;
        }

        var existente = await context.GeneroLoggers
            .FirstOrDefaultAsync(g => g.Genero == genero, cancellationToken);

        if (existente is not null)
        {
            return existente;
        }

        var novo = new GeneroLogger
        {
            Genero = genero,
            UltimaAtualizacao = DateTime.UtcNow
        };

        await context.GeneroLoggers.AddAsync(novo, cancellationToken);
        return novo;
    }

    private async Task<JogoLogger> ObterOuCriarJogoLoggerInternoAsync(int idJogo, CancellationToken cancellationToken)
    {
        var local = context.JogoLoggers.Local.FirstOrDefault(j => j.IdJogo == idJogo);
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

    private async Task<UsuarioLogger> ObterOuCriarUsuarioLoggerInternoAsync(int idUsuario, CancellationToken cancellationToken)
    {
        var local = context.UsuarioLoggers.Local.FirstOrDefault(u => u.IdUsuario == idUsuario);
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

    private static ulong AplicarDelta(ulong atual, int delta)
    {
        if (delta == 0)
        {
            return atual;
        }

        if (delta > 0)
        {
            return atual + (ulong)delta;
        }

        var decrescimo = (ulong)(-delta);
        return atual > decrescimo ? atual - decrescimo : 0UL;
    }
}

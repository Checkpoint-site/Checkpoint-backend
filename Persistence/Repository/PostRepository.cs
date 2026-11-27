using Domain.Dtos.Posts.Commands;
using Domain.Dtos.Posts.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository;

public class PostRepository(ApplicationDbContext context) : IPostRepository
{
    public async Task<PostDetalheRespostDto?> ObterPorIdAsync(
        int idPost,
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default)
    {
        var item = await (
            from post in context.Posts.AsNoTracking()
            join usuario in context.Usuarios.AsNoTracking() on post.IdUsuario equals usuario.Id
            join jogo in context.Jogos.AsNoTracking() on post.IdJogo equals jogo.IdJogo
            where post.IdPost == idPost && !post.PostDeletado && !usuario.ContaDeletada
            select new { Post = post, Usuario = usuario, Jogo = jogo })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var curtido = idUsuarioAtual.HasValue && await context.CurtidasPosts
            .AsNoTracking()
            .AnyAsync(c => c.IdPost == idPost && c.IdUsuario == idUsuarioAtual.Value, cancellationToken);

        return MapearPostDetalhe(item.Post, item.Usuario, item.Jogo, curtido, idUsuarioAtual);
    }

    public async Task<IReadOnlyList<PostDetalheRespostDto>> ListarRecentesAsync(
        int limite = 20,
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default)
    {
        var quantidade = Math.Clamp(limite, 1, 100);

        var itens = await (
            from post in context.Posts.AsNoTracking()
            join usuario in context.Usuarios.AsNoTracking() on post.IdUsuario equals usuario.Id
            join jogo in context.Jogos.AsNoTracking() on post.IdJogo equals jogo.IdJogo
            where !post.PostDeletado && !usuario.ContaDeletada
            orderby post.DataCriacao descending
            select new { Post = post, Usuario = usuario, Jogo = jogo })
            .Take(quantidade)
            .ToListAsync(cancellationToken);

        return await HidratarCurtidasEMapearAsync(itens.Select(i => (i.Post, i.Usuario, i.Jogo)).ToList(), idUsuarioAtual, cancellationToken);
    }

    public async Task<IReadOnlyList<PostDetalheRespostDto>> ListarPorJogoAsync(
        int idJogo,
        string? ordenacao = null,
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default)
    {
        var query =
            from post in context.Posts.AsNoTracking()
            join usuario in context.Usuarios.AsNoTracking() on post.IdUsuario equals usuario.Id
            join jogo in context.Jogos.AsNoTracking() on post.IdJogo equals jogo.IdJogo
            where post.IdJogo == idJogo && !post.PostDeletado && !usuario.ContaDeletada
            select new { Post = post, Usuario = usuario, Jogo = jogo };

        query = ordenacao?.Trim().ToLowerInvariant() switch
        {
            "curtidas" => query.OrderByDescending(x => x.Post.QuantidadeCurtidas).ThenByDescending(x => x.Post.DataCriacao),
            "nota" => query.OrderByDescending(x => x.Post.Fogueiras).ThenByDescending(x => x.Post.DataCriacao),
            _ => query.OrderByDescending(x => x.Post.DataCriacao)
        };

        var itens = await query.Take(100).ToListAsync(cancellationToken);
        return await HidratarCurtidasEMapearAsync(itens.Select(i => (i.Post, i.Usuario, i.Jogo)).ToList(), idUsuarioAtual, cancellationToken);
    }

    public async Task<IReadOnlyList<PostDetalheRespostDto>> ListarPorUsuarioAsync(
        int idUsuario,
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default)
    {
        var itens = await (
            from post in context.Posts.AsNoTracking()
            join usuario in context.Usuarios.AsNoTracking() on post.IdUsuario equals usuario.Id
            join jogo in context.Jogos.AsNoTracking() on post.IdJogo equals jogo.IdJogo
            where post.IdUsuario == idUsuario && !post.PostDeletado && !usuario.ContaDeletada
            orderby post.DataCriacao descending
            select new { Post = post, Usuario = usuario, Jogo = jogo })
            .Take(100)
            .ToListAsync(cancellationToken);

        return await HidratarCurtidasEMapearAsync(itens.Select(i => (i.Post, i.Usuario, i.Jogo)).ToList(), idUsuarioAtual, cancellationToken);
    }

    public async Task<(PostDetalheRespostDto Post, bool FoiCriadoNovo)> SalvarOuAtualizarDoUsuarioNoJogoAsync(
        SalvarPostCommandDto command,
        CancellationToken cancellationToken = default)
    {
        if (double.IsNaN(command.Fogueiras) || double.IsInfinity(command.Fogueiras) || command.Fogueiras < 0.5 || command.Fogueiras > 5.0)
        {
            throw new ValidationException("A nota de fogueiras deve estar entre 0.5 e 5.0.");
        }

        if (string.IsNullOrWhiteSpace(command.Conteudo))
        {
            throw new ValidationException("O conteúdo da review não pode ser vazio.");
        }

        var conteudoLimpo = command.Conteudo.Trim();
        if (conteudoLimpo.Length > 4000)
        {
            throw new ValidationException("O conteúdo da review deve ter no máximo 4000 caracteres.");
        }

        var usuario = await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == command.IdUsuario && !u.ContaDeletada, cancellationToken)
            ?? throw new NotFoundException("Usuário autor não encontrado.");

        if (usuario.SuspensoAte.HasValue && usuario.SuspensoAte.Value > DateTime.UtcNow)
        {
            throw new ForbiddenException("Contas suspensas não podem publicar ou editar reviews.");
        }

        var jogo = await context.Jogos
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.IdJogo == command.IdJogo, cancellationToken)
            ?? throw new NotFoundException("Jogo não encontrado para publicação da review.");

        var existente = await context.Posts
            .Where(p => p.IdUsuario == command.IdUsuario && p.IdJogo == command.IdJogo && !p.PostDeletado)
            .OrderByDescending(p => p.DataCriacao)
            .FirstOrDefaultAsync(cancellationToken);

        bool criadoNovo = false;
        var tituloFinal = ResolverTituloPost(command.Titulo, conteudoLimpo, jogo.Nome);
        var notaNormalizada = Math.Round(command.Fogueiras * 2.0) / 2.0;
        var tagsNormalizadas = NormalizarTagsPost(command.Tags);

        if (existente is not null)
        {
            existente.Titulo = tituloFinal;
            existente.Fogueiras = notaNormalizada;
            existente.Conteudo = conteudoLimpo;
            existente.StatusJornada = command.StatusJornada;
            existente.Tags = tagsNormalizadas;
            existente.TemSpoiler = command.TemSpoiler;
            existente.DataAtualizacao = DateTime.UtcNow;
            existente.TipoPost = command.GenerosJogo.Distinct().ToList();
        }
        else
        {
            criadoNovo = true;
            existente = new Post
            {
                IdUsuario = command.IdUsuario,
                IdJogo = command.IdJogo,
                Titulo = tituloFinal,
                Fogueiras = notaNormalizada,
                Conteudo = conteudoLimpo,
                TipoPost = command.GenerosJogo.Distinct().ToList(),
                StatusJornada = command.StatusJornada,
                Tags = tagsNormalizadas,
                TemSpoiler = command.TemSpoiler,
                DataCriacao = DateTime.UtcNow
            };

            await context.Posts.AddAsync(existente, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        var curtido = await context.CurtidasPosts
            .AsNoTracking()
            .AnyAsync(c => c.IdPost == existente.IdPost && c.IdUsuario == command.IdUsuario, cancellationToken);

        return (MapearPostDetalhe(existente, usuario, jogo, curtido, command.IdUsuario), criadoNovo);
    }

    public async Task<bool> ExcluirAsync(
        int idPost,
        int idUsuarioSolicitante,
        bool podeModerar = false,
        CancellationToken cancellationToken = default)
    {
        var post = await context.Posts
            .FirstOrDefaultAsync(p => p.IdPost == idPost && !p.PostDeletado, cancellationToken)
            ?? throw new NotFoundException("Review não encontrada.");

        if (post.IdUsuario != idUsuarioSolicitante && !podeModerar)
        {
            throw new ForbiddenException("Você só pode excluir suas próprias reviews.");
        }

        post.PostDeletado = true;
        post.DataAtualizacao = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ResultadoCurtidaRespostDto> AlternarCurtidaPostAsync(
        int idPost,
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        var post = await context.Posts
            .FirstOrDefaultAsync(p => p.IdPost == idPost && !p.PostDeletado, cancellationToken)
            ?? throw new NotFoundException("Review não encontrada.");

        var curtidaExistente = await context.CurtidasPosts
            .FirstOrDefaultAsync(c => c.IdPost == idPost && c.IdUsuario == idUsuario, cancellationToken);

        bool agoraCurtido;
        int delta;

        if (curtidaExistente is null)
        {
            await context.CurtidasPosts.AddAsync(new CurtidaPost
            {
                IdPost = idPost,
                IdUsuario = idUsuario,
                DataCriacao = DateTime.UtcNow
            }, cancellationToken);

            post.QuantidadeCurtidas += 1;
            agoraCurtido = true;
            delta = 1;
        }
        else
        {
            context.CurtidasPosts.Remove(curtidaExistente);
            post.QuantidadeCurtidas = Math.Max(0, post.QuantidadeCurtidas - 1);
            agoraCurtido = false;
            delta = -1;
        }

        await AtualizarLoggersCurtidaPostAsync(post, idUsuario, delta, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new ResultadoCurtidaRespostDto
        {
            IdAlvo = idPost,
            Curtido = agoraCurtido,
            QuantidadeCurtidas = post.QuantidadeCurtidas
        };
    }

    public async Task<IReadOnlyList<ComentarioRespostDto>> ListarComentariosPorPostAsync(
        int idPost,
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default)
    {
        var itens = await (
            from comentario in context.Comentarios.AsNoTracking()
            join usuario in context.Usuarios.AsNoTracking() on comentario.IdUsuario equals usuario.Id
            where comentario.IdPost == idPost && !comentario.ComentarioDeletado && !usuario.ContaDeletada
            orderby comentario.DataCriacao descending
            select new { Comentario = comentario, Usuario = usuario })
            .Take(200)
            .ToListAsync(cancellationToken);

        if (itens.Count == 0)
        {
            return [];
        }

        HashSet<int> curtidosSet = [];
        if (idUsuarioAtual.HasValue)
        {
            var idsComentarios = itens.Select(i => i.Comentario.IdComentario).ToList();
            var idsCurtidos = await context.CurtidasComentarios
                .AsNoTracking()
                .Where(c => c.IdUsuario == idUsuarioAtual.Value && idsComentarios.Contains(c.IdComentario))
                .Select(c => c.IdComentario)
                .ToListAsync(cancellationToken);
            curtidosSet = idsCurtidos.ToHashSet();
        }

        return itens
            .Select(i => new ComentarioRespostDto
            {
                IdComentario = i.Comentario.IdComentario,
                IdPost = i.Comentario.IdPost,
                IdUsuario = i.Usuario.Id,
                NomeAutor = i.Usuario.Nome,
                FotoPerfilAutor = i.Usuario.FotoPerfil,
                Conteudo = i.Comentario.Conteudo,
                QuantidadeCurtidas = i.Comentario.QuantidadeCurtidas,
                TemSpoiler = i.Comentario.TemSpoiler,
                CurtidoPeloUsuarioAtual = curtidosSet.Contains(i.Comentario.IdComentario),
                DoUsuarioAtual = idUsuarioAtual.HasValue && idUsuarioAtual.Value == i.Usuario.Id,
                DataCriacao = i.Comentario.DataCriacao
            })
            .ToList();
    }

    public async Task<ComentarioRespostDto> AdicionarComentarioAsync(
        CriarComentarioCommandDto command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Conteudo))
        {
            throw new ValidationException("O texto do comentário não pode ser vazio.");
        }

        var textoLimpo = command.Conteudo.Trim();
        if (textoLimpo.Length > 2000)
        {
            throw new ValidationException("O comentário deve ter no máximo 2000 caracteres.");
        }

        var usuario = await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == command.IdUsuario && !u.ContaDeletada, cancellationToken)
            ?? throw new NotFoundException("Usuário autor não encontrado.");

        if (usuario.SuspensoAte.HasValue && usuario.SuspensoAte.Value > DateTime.UtcNow)
        {
            throw new ForbiddenException("Contas suspensas não podem publicar comentários.");
        }

        var post = await context.Posts
            .FirstOrDefaultAsync(p => p.IdPost == command.IdPost && !p.PostDeletado, cancellationToken)
            ?? throw new NotFoundException("Review não encontrada para comentar.");

        var novoComentario = new Comentario
        {
            IdPost = command.IdPost,
            IdUsuario = command.IdUsuario,
            Conteudo = textoLimpo,
            TemSpoiler = command.TemSpoiler,
            DataCriacao = DateTime.UtcNow
        };

        await context.Comentarios.AddAsync(novoComentario, cancellationToken);
        post.QuantidadeComentarios += 1;

        await AtualizarLoggersComentarioAsync(post, command.IdUsuario, delta: 1, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new ComentarioRespostDto
        {
            IdComentario = novoComentario.IdComentario,
            IdPost = novoComentario.IdPost,
            IdUsuario = usuario.Id,
            NomeAutor = usuario.Nome,
            FotoPerfilAutor = usuario.FotoPerfil,
            Conteudo = novoComentario.Conteudo,
            QuantidadeCurtidas = 0,
            TemSpoiler = novoComentario.TemSpoiler,
            CurtidoPeloUsuarioAtual = false,
            DoUsuarioAtual = true,
            DataCriacao = novoComentario.DataCriacao
        };
    }

    public async Task<ResultadoCurtidaRespostDto> AlternarCurtidaComentarioAsync(
        int idComentario,
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        var comentario = await context.Comentarios
            .FirstOrDefaultAsync(c => c.IdComentario == idComentario && !c.ComentarioDeletado, cancellationToken)
            ?? throw new NotFoundException("Comentário não encontrado.");

        var existente = await context.CurtidasComentarios
            .FirstOrDefaultAsync(c => c.IdComentario == idComentario && c.IdUsuario == idUsuario, cancellationToken);

        bool agoraCurtido;
        int delta;

        if (existente is null)
        {
            await context.CurtidasComentarios.AddAsync(new CurtidaComentario
            {
                IdComentario = idComentario,
                IdUsuario = idUsuario,
                DataCriacao = DateTime.UtcNow
            }, cancellationToken);

            comentario.QuantidadeCurtidas += 1;
            agoraCurtido = true;
            delta = 1;
        }
        else
        {
            context.CurtidasComentarios.Remove(existente);
            comentario.QuantidadeCurtidas = Math.Max(0, comentario.QuantidadeCurtidas - 1);
            agoraCurtido = false;
            delta = -1;
        }

        var autorLogger = await ObterOuCriarUsuarioLoggerAsync(comentario.IdUsuario, cancellationToken);
        autorLogger.CurtidasRecebidas = AplicarDelta(autorLogger.CurtidasRecebidas, delta);
        autorLogger.UltimaAtualizacao = DateTime.UtcNow;

        var curtiuLogger = await ObterOuCriarUsuarioLoggerAsync(idUsuario, cancellationToken);
        curtiuLogger.QuantidadeCurtida = AplicarDelta(curtiuLogger.QuantidadeCurtida, delta);
        curtiuLogger.UltimaAtualizacao = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new ResultadoCurtidaRespostDto
        {
            IdAlvo = idComentario,
            Curtido = agoraCurtido,
            QuantidadeCurtidas = comentario.QuantidadeCurtidas
        };
    }

    public async Task<IReadOnlyList<FaixaDistribuicaoFogueirasDto>> ObterDistribuicaoFogueirasJogoAsync(
        int idJogo,
        CancellationToken cancellationToken = default)
    {
        var notasDiario = await context.RegistrosDiario
            .AsNoTracking()
            .Where(r => r.IdJogo == idJogo && r.NotaFogueiras != null)
            .Select(r => new { r.IdUsuario, Nota = r.NotaFogueiras!.Value })
            .ToListAsync(cancellationToken);

        var notasPosts = await context.Posts
            .AsNoTracking()
            .Where(p => p.IdJogo == idJogo && !p.PostDeletado)
            .OrderByDescending(p => p.DataCriacao)
            .Select(p => new { p.IdUsuario, p.Fogueiras })
            .ToListAsync(cancellationToken);

        var mapa = new Dictionary<int, double>();
        foreach (var item in notasDiario)
        {
            mapa[item.IdUsuario] = item.Nota;
        }

        foreach (var post in notasPosts)
        {
            if (!mapa.ContainsKey(post.IdUsuario))
            {
                mapa[post.IdUsuario] = post.Fogueiras;
            }
        }

        var contagens = new Dictionary<int, int>
        {
            [5] = 0,
            [4] = 0,
            [3] = 0,
            [2] = 0,
            [1] = 0
        };

        foreach (var nota in mapa.Values)
        {
            var faixa = Math.Clamp((int)Math.Ceiling(nota), 1, 5);
            contagens[faixa] += 1;
        }

        var total = mapa.Count;
        return new[] { 5, 4, 3, 2, 1 }
            .Select(estrelas => new FaixaDistribuicaoFogueirasDto
            {
                Estrelas = estrelas,
                Quantidade = contagens[estrelas],
                Percentual = total > 0
                    ? (int)Math.Round((contagens[estrelas] * 100.0) / total)
                    : 0
            })
            .ToList();
    }

    private async Task<IReadOnlyList<PostDetalheRespostDto>> HidratarCurtidasEMapearAsync(
        IReadOnlyList<(Post Post, Usuario Usuario, Jogo Jogo)> itens,
        int? idUsuarioAtual,
        CancellationToken cancellationToken)
    {
        if (itens.Count == 0)
        {
            return [];
        }

        HashSet<int> curtidosSet = [];
        if (idUsuarioAtual.HasValue)
        {
            var idsPosts = itens.Select(i => i.Post.IdPost).ToList();
            var curtidos = await context.CurtidasPosts
                .AsNoTracking()
                .Where(c => c.IdUsuario == idUsuarioAtual.Value && idsPosts.Contains(c.IdPost))
                .Select(c => c.IdPost)
                .ToListAsync(cancellationToken);
            curtidosSet = curtidos.ToHashSet();
        }

        return itens
            .Select(i => MapearPostDetalhe(i.Post, i.Usuario, i.Jogo, curtidosSet.Contains(i.Post.IdPost), idUsuarioAtual))
            .ToList();
    }

    private static PostDetalheRespostDto MapearPostDetalhe(
        Post post,
        Usuario usuario,
        Jogo jogo,
        bool curtidoPeloUsuario,
        int? idUsuarioAtual) => new()
    {
        IdPost = post.IdPost,
        IdUsuario = usuario.Id,
        NomeAutor = usuario.Nome,
        FotoPerfilAutor = usuario.FotoPerfil,
        IdJogo = jogo.IdJogo,
        SteamAppId = jogo.SteamAppId,
        NomeJogo = jogo.Nome,
        AnoLancamentoJogo = jogo.AnoLancamento,
        ImagemJogo = jogo.Imagem,
        DesenvolvedoraJogo = jogo.Desenvolvedora,
        Titulo = post.Titulo,
        Fogueiras = post.Fogueiras,
        Conteudo = post.Conteudo,
        StatusJornada = post.StatusJornada,
        Tags = post.Tags.ToList(),
        QuantidadeCurtidas = post.QuantidadeCurtidas,
        QuantidadeComentarios = post.QuantidadeComentarios,
        TemSpoiler = post.TemSpoiler,
        CurtidoPeloUsuarioAtual = curtidoPeloUsuario,
        DoUsuarioAtual = idUsuarioAtual.HasValue && idUsuarioAtual.Value == usuario.Id,
        DataCriacao = post.DataCriacao,
        DataAtualizacao = post.DataAtualizacao
    };

    private async Task AtualizarLoggersCurtidaPostAsync(Post post, int idUsuarioCurtiu, int delta, CancellationToken cancellationToken)
    {
        var agora = DateTime.UtcNow;

        var jogoLogger = await ObterOuCriarJogoLoggerAsync(post.IdJogo, cancellationToken);
        jogoLogger.QuantidadeCurtida = AplicarDelta(jogoLogger.QuantidadeCurtida, delta);
        jogoLogger.UltimaAtualizacao = agora;

        var autorLogger = await ObterOuCriarUsuarioLoggerAsync(post.IdUsuario, cancellationToken);
        autorLogger.CurtidasRecebidas = AplicarDelta(autorLogger.CurtidasRecebidas, delta);
        autorLogger.UltimaAtualizacao = agora;

        var curtiuLogger = await ObterOuCriarUsuarioLoggerAsync(idUsuarioCurtiu, cancellationToken);
        curtiuLogger.QuantidadeCurtida = AplicarDelta(curtiuLogger.QuantidadeCurtida, delta);
        curtiuLogger.UltimaAtualizacao = agora;

        foreach (var genero in post.TipoPost.Distinct())
        {
            var generoLogger = await ObterOuCriarGeneroLoggerAsync(genero, cancellationToken);
            generoLogger.QuantidadeCurtida = AplicarDelta(generoLogger.QuantidadeCurtida, delta);
            generoLogger.UltimaAtualizacao = agora;
        }
    }

    private async Task AtualizarLoggersComentarioAsync(Post post, int idUsuarioComentou, int delta, CancellationToken cancellationToken)
    {
        var agora = DateTime.UtcNow;

        var jogoLogger = await ObterOuCriarJogoLoggerAsync(post.IdJogo, cancellationToken);
        jogoLogger.PresencaComentario = AplicarDelta(jogoLogger.PresencaComentario, delta);
        jogoLogger.UltimaAtualizacao = agora;

        var usuarioLogger = await ObterOuCriarUsuarioLoggerAsync(idUsuarioComentou, cancellationToken);
        usuarioLogger.PresencaComentario = AplicarDelta(usuarioLogger.PresencaComentario, delta);
        usuarioLogger.UltimaAtualizacao = agora;

        foreach (var genero in post.TipoPost.Distinct())
        {
            var generoLogger = await ObterOuCriarGeneroLoggerAsync(genero, cancellationToken);
            generoLogger.PresencaComentario = AplicarDelta(generoLogger.PresencaComentario, delta);
            generoLogger.UltimaAtualizacao = agora;
        }
    }

    private async Task<JogoLogger> ObterOuCriarJogoLoggerAsync(int idJogo, CancellationToken cancellationToken)
    {
        var local = context.JogoLoggers.Local.FirstOrDefault(j => j.IdJogo == idJogo);
        if (local is not null)
        {
            return local;
        }

        var existente = await context.JogoLoggers.FirstOrDefaultAsync(j => j.IdJogo == idJogo, cancellationToken);
        if (existente is not null)
        {
            return existente;
        }

        var novo = new JogoLogger { IdJogo = idJogo, UltimaAtualizacao = DateTime.UtcNow };
        await context.JogoLoggers.AddAsync(novo, cancellationToken);
        return novo;
    }

    private async Task<UsuarioLogger> ObterOuCriarUsuarioLoggerAsync(int idUsuario, CancellationToken cancellationToken)
    {
        var local = context.UsuarioLoggers.Local.FirstOrDefault(u => u.IdUsuario == idUsuario);
        if (local is not null)
        {
            return local;
        }

        var existente = await context.UsuarioLoggers.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, cancellationToken);
        if (existente is not null)
        {
            return existente;
        }

        var novo = new UsuarioLogger { IdUsuario = idUsuario, UltimaAtualizacao = DateTime.UtcNow };
        await context.UsuarioLoggers.AddAsync(novo, cancellationToken);
        return novo;
    }

    private async Task<GeneroLogger> ObterOuCriarGeneroLoggerAsync(Generos genero, CancellationToken cancellationToken)
    {
        var local = context.GeneroLoggers.Local.FirstOrDefault(g => g.Genero == genero);
        if (local is not null)
        {
            return local;
        }

        var existente = await context.GeneroLoggers.FirstOrDefaultAsync(g => g.Genero == genero, cancellationToken);
        if (existente is not null)
        {
            return existente;
        }

        var novo = new GeneroLogger { Genero = genero, UltimaAtualizacao = DateTime.UtcNow };
        await context.GeneroLoggers.AddAsync(novo, cancellationToken);
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

    private static string ResolverTituloPost(string? titulo, string conteudoLimpo, string nomeJogo)
    {
        if (!string.IsNullOrWhiteSpace(titulo))
        {
            var limpo = titulo.Trim();
            return limpo[..Math.Min(180, limpo.Length)];
        }

        if (!string.IsNullOrWhiteSpace(conteudoLimpo))
        {
            var primeiraLinha = conteudoLimpo.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            return primeiraLinha.Length <= 80
                ? primeiraLinha
                : $"{primeiraLinha[..77].TrimEnd()}...";
        }

        return $"Relato sobre {nomeJogo.Trim()}";
    }

    private static List<string> NormalizarTagsPost(IEnumerable<string>? tags) =>
        tags is null
            ? []
            : tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Where(t => t.Length <= 40)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToList();
}

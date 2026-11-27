using Application.Dtos.Posts.Requests;
using Application.Dtos.Posts.Responses;
using Application.Interfaces;
using Application.Mappers;
using Application.Utils;
using Domain.Dtos.Diario.Commands;
using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Posts.Commands;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;

namespace Application.Services;

public class PostService(
    IPostRepository postRepository,
    IDiarioRepository diarioRepository,
    ICatalogoRepository catalogoRepository,
    IEngajamentoLoggerRepository loggerRepository,
    ISteamStoreService steamStoreService,
    ICatalogoCacheService cacheService
) : IPostService
{
    private static readonly TimeSpan TtlOfertaTemporaria = TimeSpan.FromMinutes(30);

    public async Task<IReadOnlyList<PostResponseDto>> ListarRecentesAsync(
        int limite = 20,
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default)
    {
        if (limite <= 0 || limite > 100)
        {
            throw new ValidationException("O limite deve estar entre 1 e 100 reviews.");
        }

        var posts = await postRepository.ListarRecentesAsync(limite, idUsuarioAtual, cancellationToken);
        return posts.Select(PostMapper.ToPostResponse).ToList();
    }

    public async Task<PostComComentariosResponseDto> ObterDetalhesComComentariosAsync(
        int idPost,
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default)
    {
        if (idPost <= 0)
        {
            throw new ValidationException("O identificador da review deve ser positivo.");
        }

        var post = await postRepository.ObterPorIdAsync(idPost, idUsuarioAtual, cancellationToken)
            ?? throw new NotFoundException("Review não encontrada.");

        var comentarios = await postRepository.ListarComentariosPorPostAsync(idPost, idUsuarioAtual, cancellationToken);
        var recentes = await postRepository.ListarRecentesAsync(8, idUsuarioAtual, cancellationToken);

        var outrasReviews = recentes
            .Where(r => r.IdPost != idPost)
            .Take(4)
            .Select(PostMapper.ToPostResponse)
            .ToList();

        return new PostComComentariosResponseDto
        {
            Review = PostMapper.ToPostResponse(post),
            Comentarios = comentarios.Select(PostMapper.ToComentarioResponse).ToList(),
            OutrasReviews = outrasReviews
        };
    }

    public async Task<IReadOnlyList<PostResponseDto>> ListarPorJogoSteamAppIdAsync(
        int steamAppId,
        string? ordenacao = null,
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default)
    {
        if (steamAppId <= 0)
        {
            throw new ValidationException("O identificador SteamAppId deve ser positivo.");
        }

        var jogo = await catalogoRepository.ObterPorSteamAppIdAsync(steamAppId, cancellationToken);
        if (jogo is null)
        {
            return [];
        }

        var posts = await postRepository.ListarPorJogoAsync(jogo.IdJogo, ordenacao, idUsuarioAtual, cancellationToken);
        return posts.Select(PostMapper.ToPostResponse).ToList();
    }

    public async Task<PostResponseDto> SalvarPorJogoSteamAppIdAsync(
        int idUsuario,
        int steamAppId,
        SalvarPostRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (steamAppId <= 0)
        {
            throw new ValidationException("O identificador SteamAppId deve ser positivo.");
        }

        var notaArredondada = AvaliacaoFogueiraUtil.ArredondarMeiaChama(request.Nota);
        var conteudoLimpo = AvaliacaoFogueiraUtil.ValidarENormalizarConteudoReview(request.Comentario);
        var tagsNormalizadas = AvaliacaoFogueiraUtil.NormalizarTags(request.Tags);

        var status = StatusJornadaHelper.ConverterSlugOpcional(request.Status);
        var jogo = await ObterOuSincronizarJogoAsync(steamAppId, cancellationToken);
        var tituloFinal = AvaliacaoFogueiraUtil.ResolverTituloReview(request.Titulo, conteudoLimpo, jogo.Nome);

        var (postSalvo, criadoNovo) = await postRepository.SalvarOuAtualizarDoUsuarioNoJogoAsync(
            new SalvarPostCommandDto
            {
                IdUsuario = idUsuario,
                IdJogo = jogo.IdJogo,
                Titulo = tituloFinal,
                Fogueiras = notaArredondada,
                Conteudo = conteudoLimpo,
                GenerosJogo = jogo.Generos,
                StatusJornada = status,
                Tags = tagsNormalizadas,
                TemSpoiler = request.ContemSpoiler
            },
            cancellationToken);

        await diarioRepository.SalvarOuAtualizarAsync(
            new SalvarAvaliacaoDiarioCommandDto
            {
                IdUsuario = idUsuario,
                IdJogo = jogo.IdJogo,
                Status = status,
                NotaFogueiras = notaArredondada,
                Jogado = true,
                Tags = tagsNormalizadas
            },
            cancellationToken);

        await loggerRepository.SincronizarAvaliacoesJogoEUsuarioAsync(
            jogo.IdJogo,
            idUsuario,
            jogo.Generos,
            deltaPostsGenero: criadoNovo ? 1 : 0,
            cancellationToken);

        return PostMapper.ToPostResponse(postSalvo);
    }

    public async Task ExcluirAsync(
        int idPost,
        int idUsuarioSolicitante,
        string? roleSolicitante = null,
        CancellationToken cancellationToken = default)
    {
        var postExistente = await postRepository.ObterPorIdAsync(idPost, idUsuarioSolicitante, cancellationToken)
            ?? throw new NotFoundException("Review não encontrada.");

        var podeModerar = string.Equals(roleSolicitante, "Moderador", StringComparison.OrdinalIgnoreCase)
            || string.Equals(roleSolicitante, "Admin", StringComparison.OrdinalIgnoreCase);

        await postRepository.ExcluirAsync(idPost, idUsuarioSolicitante, podeModerar, cancellationToken);

        var jogo = await catalogoRepository.ObterPorIdAsync(postExistente.IdJogo, cancellationToken);
        await loggerRepository.SincronizarAvaliacoesJogoEUsuarioAsync(
            postExistente.IdJogo,
            postExistente.IdUsuario,
            jogo?.Generos ?? [],
            deltaPostsGenero: -1,
            cancellationToken);
    }

    public async Task<CurtidaResponseDto> AlternarCurtidaPostAsync(
        int idPost,
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        if (idPost <= 0)
        {
            throw new ValidationException("O identificador da review deve ser positivo.");
        }

        var resultado = await postRepository.AlternarCurtidaPostAsync(idPost, idUsuario, cancellationToken);
        return PostMapper.ToCurtidaResponse(resultado);
    }

    public async Task<IReadOnlyList<ComentarioResponseDto>> ListarComentariosAsync(
        int idPost,
        int? idUsuarioAtual = null,
        CancellationToken cancellationToken = default)
    {
        if (idPost <= 0)
        {
            throw new ValidationException("O identificador da review deve ser positivo.");
        }

        var comentarios = await postRepository.ListarComentariosPorPostAsync(idPost, idUsuarioAtual, cancellationToken);
        return comentarios.Select(PostMapper.ToComentarioResponse).ToList();
    }

    public async Task<ComentarioResponseDto> AdicionarComentarioAsync(
        int idPost,
        int idUsuario,
        CriarComentarioRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (idPost <= 0)
        {
            throw new ValidationException("O identificador da review deve ser positivo.");
        }

        var comentario = await postRepository.AdicionarComentarioAsync(
            new CriarComentarioCommandDto
            {
                IdPost = idPost,
                IdUsuario = idUsuario,
                Conteudo = request.Texto,
                TemSpoiler = request.ContemSpoiler
            },
            cancellationToken);

        return PostMapper.ToComentarioResponse(comentario);
    }

    public async Task<CurtidaResponseDto> AlternarCurtidaComentarioAsync(
        int idComentario,
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        if (idComentario <= 0)
        {
            throw new ValidationException("O identificador do comentário deve ser positivo.");
        }

        var resultado = await postRepository.AlternarCurtidaComentarioAsync(idComentario, idUsuario, cancellationToken);
        return PostMapper.ToCurtidaResponse(resultado);
    }

    private async Task<JogoCatalogoRespostDto> ObterOuSincronizarJogoAsync(
        int steamAppId,
        CancellationToken cancellationToken)
    {
        var existente = await catalogoRepository.ObterPorSteamAppIdAsync(steamAppId, cancellationToken);
        if (existente is not null)
        {
            return existente;
        }

        var detalhes = await steamStoreService.ObterDetalhesJogoAsync(steamAppId, cancellationToken)
            ?? throw new NotFoundException($"Jogo com SteamAppId {steamAppId} não foi encontrado na Steam.");

        var oferta = CatalogoService.ExtrairOfertaTemporaria(detalhes);
        await cacheService.SalvarAsync(CatalogoService.ObterChaveOferta(steamAppId), oferta, TtlOfertaTemporaria, cancellationToken);

        return await catalogoRepository.SalvarOuAtualizarAsync(detalhes, cancellationToken);
    }
}

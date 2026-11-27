using Application.Dtos.Catalogo.Responses;
using Application.Dtos.Diario.Requests;
using Application.Dtos.Diario.Responses;
using Application.Dtos.Posts.Responses;
using Application.Interfaces;
using Application.Mappers;
using Application.Strategies.Algoritmos;
using Application.Utils;
using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Diario.Commands;
using Domain.Dtos.Estantes.Commands;
using Domain.Dtos.Posts.Commands;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;

namespace Application.Services;

public class DiarioService(
    IDiarioRepository diarioRepository,
    IPostRepository postRepository,
    IEstanteRepository estanteRepository,
    ICatalogoRepository catalogoRepository,
    IEngajamentoLoggerRepository loggerRepository,
    ISteamStoreService steamStoreService,
    ICatalogoCacheService cacheService,
    IRegraEstanteFactory regraEstanteFactory
) : IDiarioService
{
    private static readonly TimeSpan TtlOfertaTemporaria = TimeSpan.FromMinutes(30);

    public async Task<BibliotecaUsuarioResponseDto> ObterMinhaBibliotecaAsync(
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        var registros = await diarioRepository.ListarPorUsuarioAsync(idUsuario, cancellationToken);
        var postsUsuario = await postRepository.ListarPorUsuarioAsync(idUsuario, idUsuario, cancellationToken);

        var notasValidas = registros
            .Where(r => r.NotaFogueiras.HasValue)
            .Select(r => r.NotaFogueiras!.Value)
            .ToList();

        var mediaUsuario = notasValidas.Count > 0
            ? Math.Round(notasValidas.Average(), 2)
            : 0.0;

        return new BibliotecaUsuarioResponseDto
        {
            IdUsuario = idUsuario,
            Resumo = new ResumoBibliotecaUsuarioResponseDto
            {
                Jogados = registros.Count(r => r.Jogado),
                QueroJogar = registros.Count(r => r.QueroJogar),
                Favoritos = registros.Count(r => r.Favorito),
                Reviews = postsUsuario.Count,
                MediaFogueirasUsuario = mediaUsuario
            },
            Registros = registros.Select(PostMapper.ToRegistroResponse).ToList(),
            ReviewsUsuario = postsUsuario.Select(PostMapper.ToPostResponse).ToList()
        };
    }

    public async Task<AvaliacaoSalvaResponseDto> SalvarAvaliacaoJogoAsync(
        int idUsuario,
        int steamAppId,
        SalvarAvaliacaoJogoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (steamAppId <= 0)
        {
            throw new ValidationException("O identificador SteamAppId deve ser positivo.");
        }

        var notaArredondada = AvaliacaoFogueiraUtil.ArredondarMeiaChama(request.Nota);
        var tagsNormalizadas = AvaliacaoFogueiraUtil.NormalizarTags(request.Tags);

        var status = StatusJornadaHelper.ConverterSlugOpcional(request.Status) ?? StatusJornada.Zerado;
        var jogo = await ObterOuSincronizarJogoAsync(steamAppId, cancellationToken);

        bool? jogadoExplicito = status is StatusJornada.Zerado or StatusJornada.Jogando ? true : null;

        await diarioRepository.SalvarOuAtualizarAsync(
            new SalvarAvaliacaoDiarioCommandDto
            {
                IdUsuario = idUsuario,
                IdJogo = jogo.IdJogo,
                Status = status,
                NotaFogueiras = notaArredondada,
                Jogado = jogadoExplicito,
                Tags = tagsNormalizadas
            },
            cancellationToken);

        if (request.Favorito.HasValue)
        {
            await SincronizarFavoritoOpcionalAsync(idUsuario, jogo, request.Favorito.Value, cancellationToken);
        }

        if (status == StatusJornada.QueroJogar)
        {
            await SincronizarPretendeJogarSeAusenteAsync(idUsuario, jogo, cancellationToken);
        }

        PostResponseDto? reviewPublicada = null;
        int deltaPostsGenero = 0;

        if (!string.IsNullOrWhiteSpace(request.Comentario))
        {
            var conteudoLimpo = AvaliacaoFogueiraUtil.ValidarENormalizarConteudoReview(request.Comentario);
            var (postSalvo, criadoNovo) = await postRepository.SalvarOuAtualizarDoUsuarioNoJogoAsync(
                new SalvarPostCommandDto
                {
                    IdUsuario = idUsuario,
                    IdJogo = jogo.IdJogo,
                    Titulo = AvaliacaoFogueiraUtil.GerarTituloAutomatico(conteudoLimpo, jogo.Nome),
                    Fogueiras = notaArredondada,
                    Conteudo = conteudoLimpo,
                    GenerosJogo = jogo.Generos,
                    StatusJornada = status,
                    Tags = tagsNormalizadas,
                    TemSpoiler = request.ContemSpoiler
                },
                cancellationToken);

            reviewPublicada = PostMapper.ToPostResponse(postSalvo);
            if (criadoNovo)
            {
                deltaPostsGenero = 1;
            }
        }

        await loggerRepository.SincronizarAvaliacoesJogoEUsuarioAsync(
            jogo.IdJogo,
            idUsuario,
            jogo.Generos,
            deltaPostsGenero,
            cancellationToken);

        var registroFinal = await diarioRepository.ObterPorUsuarioEJogoAsync(idUsuario, jogo.IdJogo, cancellationToken)
            ?? throw new NotFoundException("Não foi possível carregar o registro salvo.");

        var loggerJogo = await loggerRepository.ObterJogoLoggerAsync(jogo.IdJogo, cancellationToken);
        var distribuicao = await postRepository.ObterDistribuicaoFogueirasJogoAsync(jogo.IdJogo, cancellationToken);
        var mediaFogueiras = ContextoCuradoria.CalcularMediaFogueiras(loggerJogo);
        var totalAvaliacoes = loggerJogo is { TotalAvaliacoes: > 0 }
            ? (int)loggerJogo.TotalAvaliacoes
            : registroFinal.TotalAvaliacoesJogo;

        return new AvaliacaoSalvaResponseDto
        {
            Registro = PostMapper.ToRegistroResponse(registroFinal),
            ReviewPublicada = reviewPublicada,
            MediaFogueirasJogo = mediaFogueiras,
            TotalAvaliacoesJogo = totalAvaliacoes,
            DistribuicaoFogueiras = distribuicao
                .Select(f => new FaixaDistribuicaoFogueirasResponseDto
                {
                    Estrelas = f.Estrelas,
                    Quantidade = f.Quantidade,
                    Percentual = f.Percentual
                })
                .ToList()
        };
    }

    public async Task<RegistroJogoUsuarioResponseDto> AlternarJogadoAsync(
        int idUsuario,
        int steamAppId,
        CancellationToken cancellationToken = default)
    {
        if (steamAppId <= 0)
        {
            throw new ValidationException("O identificador SteamAppId deve ser positivo.");
        }

        var jogo = await ObterOuSincronizarJogoAsync(steamAppId, cancellationToken);
        var atual = await diarioRepository.ObterPorUsuarioEJogoAsync(idUsuario, jogo.IdJogo, cancellationToken);
        var proximoJogado = !(atual?.Jogado ?? false);
        var proximoStatus = proximoJogado
            ? (atual?.Status ?? StatusJornada.Zerado)
            : atual?.Status;

        var salvo = await diarioRepository.SalvarOuAtualizarAsync(
            new SalvarAvaliacaoDiarioCommandDto
            {
                IdUsuario = idUsuario,
                IdJogo = jogo.IdJogo,
                Status = proximoStatus,
                NotaFogueiras = atual?.NotaFogueiras,
                Jogado = proximoJogado,
                Tags = atual?.Tags
            },
            cancellationToken);

        return PostMapper.ToRegistroResponse(salvo);
    }

    private async Task SincronizarFavoritoOpcionalAsync(
        int idUsuario,
        JogoCatalogoRespostDto jogo,
        bool desejarFavorito,
        CancellationToken cancellationToken)
    {
        var favoritos = await estanteRepository.ObterFavoritosPorUsuarioAsync(idUsuario, cancellationToken);
        var jaEstaNosFavoritos = favoritos?.Jogos.Contains(jogo.IdJogo) == true;

        if (desejarFavorito && !jaEstaNosFavoritos)
        {
            var regraFav = regraEstanteFactory.Criar("favoritos");
            var baseFavoritos = favoritos ?? new Domain.Dtos.Estantes.Respost.FavoritosRespostDto
            {
                IdUsuario = idUsuario,
                Jogos = [],
                ExibirNoTopoPerfil = true,
                DataAtualizacao = DateTime.UtcNow
            };

            regraFav.ValidarAdicaoJogo(baseFavoritos, jogo);

            await estanteRepository.SalvarFavoritosAsync(
                new SalvarFavoritosCommandDto
                {
                    IdUsuario = idUsuario,
                    Jogos = [.. baseFavoritos.Jogos, jogo.IdJogo],
                    JogoDestaquePerfilId = baseFavoritos.JogoDestaquePerfilId,
                    ExibirNoTopoPerfil = baseFavoritos.ExibirNoTopoPerfil,
                    ObservacaoDestaque = baseFavoritos.ObservacaoDestaque
                },
                cancellationToken);

            var impacto = regraFav.CriarComandoImpactoLoggers(idUsuario, jogo, adicionando: true);
            await loggerRepository.AplicarImpactoEstanteAsync(impacto, cancellationToken);
        }
        else if (!desejarFavorito && jaEstaNosFavoritos && favoritos is not null)
        {
            var regraFav = regraEstanteFactory.Criar("favoritos");
            await estanteRepository.SalvarFavoritosAsync(
                new SalvarFavoritosCommandDto
                {
                    IdUsuario = idUsuario,
                    Jogos = favoritos.Jogos.Where(id => id != jogo.IdJogo).ToList(),
                    JogoDestaquePerfilId = favoritos.JogoDestaquePerfilId == jogo.IdJogo ? null : favoritos.JogoDestaquePerfilId,
                    ExibirNoTopoPerfil = favoritos.ExibirNoTopoPerfil,
                    ObservacaoDestaque = favoritos.ObservacaoDestaque
                },
                cancellationToken);

            var impacto = regraFav.CriarComandoImpactoLoggers(idUsuario, jogo, adicionando: false);
            await loggerRepository.AplicarImpactoEstanteAsync(impacto, cancellationToken);
        }
    }

    private async Task SincronizarPretendeJogarSeAusenteAsync(
        int idUsuario,
        JogoCatalogoRespostDto jogo,
        CancellationToken cancellationToken)
    {
        var pretende = await estanteRepository.ObterPretendeJogarPorUsuarioAsync(idUsuario, cancellationToken);
        if (pretende?.Jogos.Contains(jogo.IdJogo) == true || jogo.EmBreve)
        {
            return;
        }

        var regra = regraEstanteFactory.Criar("pretende-jogar");
        var basePretende = pretende ?? new Domain.Dtos.Estantes.Respost.PretendeJogarRespostDto
        {
            IdUsuario = idUsuario,
            Jogos = [],
            NivelPrioridade = 1,
            NotificarEmPromocao = true,
            DataAtualizacao = DateTime.UtcNow
        };

        if (basePretende.Jogos.Count >= 200)
        {
            return;
        }

        await estanteRepository.SalvarPretendeJogarAsync(
            new SalvarPretendeJogarCommandDto
            {
                IdUsuario = idUsuario,
                Jogos = [.. basePretende.Jogos, jogo.IdJogo],
                NivelPrioridade = basePretende.NivelPrioridade,
                NotificarEmPromocao = basePretende.NotificarEmPromocao,
                MetaAnoConclusao = basePretende.MetaAnoConclusao
            },
            cancellationToken);

        var impacto = regra.CriarComandoImpactoLoggers(idUsuario, jogo, adicionando: true);
        await loggerRepository.AplicarImpactoEstanteAsync(impacto, cancellationToken);
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

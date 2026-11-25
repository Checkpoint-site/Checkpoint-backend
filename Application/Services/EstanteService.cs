using Application.Dtos.Catalogo.Responses;
using Application.Dtos.Estantes.Requests;
using Application.Dtos.Estantes.Responses;
using Application.Interfaces;
using Application.Mappers;
using Application.Strategies.Algoritmos;
using Application.Strategies.Estantes;
using Domain.Dtos.Catalogo;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Domain.Model;

namespace Application.Services;

public class EstanteService(
    IEstanteRepository estanteRepository,
    ICatalogoRepository catalogoRepository,
    IEngajamentoLoggerRepository loggerRepository,
    ISteamStoreService steamStoreService,
    ICatalogoCacheService cacheService,
    IEnumerable<RegraEstanteCatalogo> regrasEstante
) : IEstanteService
{
    private static readonly TimeSpan TtlOfertaTemporaria = TimeSpan.FromMinutes(30);

    public async Task<EstantesUsuarioResponseDto> ObterEstantesUsuarioAsync(
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        var favoritos = await estanteRepository.ObterFavoritosPorUsuarioAsync(idUsuario, cancellationToken)
            ?? CriarFavoritosPadrao(idUsuario);

        var pretendeJogar = await estanteRepository.ObterPretendeJogarPorUsuarioAsync(idUsuario, cancellationToken)
            ?? CriarPretendeJogarPadrao(idUsuario);

        var todosIdsJogos = favoritos.Jogos
            .Concat(pretendeJogar.Jogos)
            .Distinct()
            .ToList();

        var jogos = await catalogoRepository.ObterPorIdsAsync(todosIdsJogos, cancellationToken);
        var mapaJogos = jogos.ToDictionary(j => j.IdJogo);
        var loggers = await loggerRepository.ObterJogoLoggersPorJogosAsync(todosIdsJogos, cancellationToken);
        var ofertas = await CarregarOfertasDoCacheAsync(jogos, cancellationToken);

        return new EstantesUsuarioResponseDto
        {
            IdUsuario = idUsuario,
            Favoritos = MapearFavoritos(favoritos, mapaJogos, loggers, ofertas),
            PretendeJogar = MapearPretendeJogar(pretendeJogar, mapaJogos, loggers, ofertas)
        };
    }

    public async Task<EstantesUsuarioResponseDto> AdicionarJogoAsync(
        int idUsuario,
        string tipoEstante,
        int steamAppId,
        CancellationToken cancellationToken = default)
    {
        if (steamAppId <= 0)
        {
            throw new ValidationException("O identificador SteamAppId deve ser um número positivo.");
        }

        var regra = ResolverRegraEstante(tipoEstante);
        var jogo = await ObterOuSincronizarJogoAsync(steamAppId, cancellationToken);

        if (regra.TipoEstante == "favoritos")
        {
            var favoritos = await estanteRepository.ObterFavoritosPorUsuarioAsync(idUsuario, cancellationToken)
                ?? CriarFavoritosPadrao(idUsuario);

            regra.ValidarAdicaoJogo(favoritos, jogo);
            favoritos.Jogos = [.. favoritos.Jogos, jogo.IdJogo];
            await estanteRepository.SalvarFavoritosAsync(favoritos, cancellationToken);
        }
        else
        {
            var pretendeJogar = await estanteRepository.ObterPretendeJogarPorUsuarioAsync(idUsuario, cancellationToken)
                ?? CriarPretendeJogarPadrao(idUsuario);

            regra.ValidarAdicaoJogo(pretendeJogar, jogo);
            pretendeJogar.Jogos = [.. pretendeJogar.Jogos, jogo.IdJogo];
            await estanteRepository.SalvarPretendeJogarAsync(pretendeJogar, cancellationToken);
        }

        await PropagarImpactoNosLoggersAsync(idUsuario, jogo, regra, adicionando: true, cancellationToken);

        return await ObterEstantesUsuarioAsync(idUsuario, cancellationToken);
    }

    public async Task<EstantesUsuarioResponseDto> RemoverJogoAsync(
        int idUsuario,
        string tipoEstante,
        int steamAppId,
        CancellationToken cancellationToken = default)
    {
        if (steamAppId <= 0)
        {
            throw new ValidationException("O identificador SteamAppId deve ser um número positivo.");
        }

        var regra = ResolverRegraEstante(tipoEstante);
        var jogo = await catalogoRepository.ObterPorSteamAppIdAsync(steamAppId, cancellationToken)
            ?? throw new NotFoundException($"Jogo com SteamAppId {steamAppId} não encontrado no catálogo.");

        if (regra.TipoEstante == "favoritos")
        {
            var favoritos = await estanteRepository.ObterFavoritosPorUsuarioAsync(idUsuario, cancellationToken);
            if (favoritos is null || !favoritos.Jogos.Contains(jogo.IdJogo))
            {
                throw new NotFoundException("Este jogo não está na sua estante de Favoritos.");
            }

            favoritos.Jogos = favoritos.Jogos.Where(id => id != jogo.IdJogo).ToList();
            if (favoritos.JogoDestaquePerfilId == jogo.IdJogo)
            {
                favoritos.JogoDestaquePerfilId = null;
            }

            await estanteRepository.SalvarFavoritosAsync(favoritos, cancellationToken);
        }
        else
        {
            var pretendeJogar = await estanteRepository.ObterPretendeJogarPorUsuarioAsync(idUsuario, cancellationToken);
            if (pretendeJogar is null || !pretendeJogar.Jogos.Contains(jogo.IdJogo))
            {
                throw new NotFoundException("Este jogo não está na sua estante 'Pretende Jogar'.");
            }

            pretendeJogar.Jogos = pretendeJogar.Jogos.Where(id => id != jogo.IdJogo).ToList();
            await estanteRepository.SalvarPretendeJogarAsync(pretendeJogar, cancellationToken);
        }

        await PropagarImpactoNosLoggersAsync(idUsuario, jogo, regra, adicionando: false, cancellationToken);

        return await ObterEstantesUsuarioAsync(idUsuario, cancellationToken);
    }

    public async Task<EstanteFavoritosResponseDto> AtualizarConfiguracaoFavoritosAsync(
        int idUsuario,
        AtualizarFavoritosConfigRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.ObservacaoDestaque is { Length: > 300 })
        {
            throw new ValidationException("A observação de destaque deve ter no máximo 300 caracteres.");
        }

        var favoritos = await estanteRepository.ObterFavoritosPorUsuarioAsync(idUsuario, cancellationToken)
            ?? CriarFavoritosPadrao(idUsuario);

        int? idJogoDestaque = null;
        if (request.JogoDestaqueSteamAppId is not null)
        {
            var jogoDestaque = await catalogoRepository.ObterPorSteamAppIdAsync(request.JogoDestaqueSteamAppId.Value, cancellationToken)
                ?? throw new NotFoundException("O jogo informado para destaque de perfil não foi encontrado.");

            if (!favoritos.Jogos.Contains(jogoDestaque.IdJogo))
            {
                throw new ValidationException("O jogo de destaque do perfil precisa estar presente na sua estante de Favoritos.");
            }

            idJogoDestaque = jogoDestaque.IdJogo;
        }

        favoritos.JogoDestaquePerfilId = idJogoDestaque;
        favoritos.ExibirNoTopoPerfil = request.ExibirNoTopoPerfil;
        favoritos.ObservacaoDestaque = request.ObservacaoDestaque?.Trim();

        await estanteRepository.SalvarFavoritosAsync(favoritos, cancellationToken);

        var jogos = await catalogoRepository.ObterPorIdsAsync(favoritos.Jogos, cancellationToken);
        var loggers = await loggerRepository.ObterJogoLoggersPorJogosAsync(favoritos.Jogos, cancellationToken);
        var ofertas = await CarregarOfertasDoCacheAsync(jogos, cancellationToken);

        return MapearFavoritos(favoritos, jogos.ToDictionary(j => j.IdJogo), loggers, ofertas);
    }

    public async Task<EstantePretendeJogarResponseDto> AtualizarConfiguracaoPretendeJogarAsync(
        int idUsuario,
        AtualizarPretendeJogarConfigRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.NivelPrioridade < 1 || request.NivelPrioridade > 5)
        {
            throw new ValidationException("O nível de prioridade deve estar entre 1 e 5.");
        }

        var anoMinimo = DateTime.UtcNow.Year;
        if (request.MetaAnoConclusao is not null && (request.MetaAnoConclusao < anoMinimo || request.MetaAnoConclusao > anoMinimo + 10))
        {
            throw new ValidationException($"A meta de ano de conclusão deve estar entre {anoMinimo} e {anoMinimo + 10}.");
        }

        var pretendeJogar = await estanteRepository.ObterPretendeJogarPorUsuarioAsync(idUsuario, cancellationToken)
            ?? CriarPretendeJogarPadrao(idUsuario);

        pretendeJogar.NivelPrioridade = request.NivelPrioridade;
        pretendeJogar.NotificarEmPromocao = request.NotificarEmPromocao;
        pretendeJogar.MetaAnoConclusao = request.MetaAnoConclusao;

        await estanteRepository.SalvarPretendeJogarAsync(pretendeJogar, cancellationToken);

        var jogos = await catalogoRepository.ObterPorIdsAsync(pretendeJogar.Jogos, cancellationToken);
        var loggers = await loggerRepository.ObterJogoLoggersPorJogosAsync(pretendeJogar.Jogos, cancellationToken);
        var ofertas = await CarregarOfertasDoCacheAsync(jogos, cancellationToken);

        return MapearPretendeJogar(pretendeJogar, jogos.ToDictionary(j => j.IdJogo), loggers, ofertas);
    }

    public async Task<PromocoesPretendeJogarResponseDto> ObterPromocoesPretendeJogarAsync(
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        var pretendeJogar = await estanteRepository.ObterPretendeJogarPorUsuarioAsync(idUsuario, cancellationToken)
            ?? CriarPretendeJogarPadrao(idUsuario);

        if (!pretendeJogar.NotificarEmPromocao || pretendeJogar.Jogos.Count == 0)
        {
            return new PromocoesPretendeJogarResponseDto
            {
                NotificacoesAtivas = pretendeJogar.NotificarEmPromocao,
                TotalEmPromocao = 0,
                JogosEmPromocao = []
            };
        }

        var jogos = await catalogoRepository.ObterPorIdsAsync(pretendeJogar.Jogos, cancellationToken);
        var emPromocao = new List<PromocaoJogoEstanteResponseDto>();

        foreach (var jogo in jogos.Where(j => !j.Gratuito && !j.EmBreve))
        {
            var chaveOferta = CatalogoService.ObterChaveOferta(jogo.SteamAppId);
            var oferta = await cacheService.ObterAsync<OfertaJogoSteamDto>(chaveOferta, cancellationToken);

            if (oferta is null)
            {
                var detalhesAoVivo = await steamStoreService.ObterDetalhesJogoAsync(jogo.SteamAppId, cancellationToken);
                if (detalhesAoVivo is not null)
                {
                    oferta = CatalogoService.ExtrairOfertaTemporaria(detalhesAoVivo);
                    await cacheService.SalvarAsync(chaveOferta, oferta, TtlOfertaTemporaria, cancellationToken);
                }
            }

            if (oferta is { PercentualDesconto: > 0 })
            {
                emPromocao.Add(new PromocaoJogoEstanteResponseDto
                {
                    IdJogo = jogo.IdJogo,
                    SteamAppId = jogo.SteamAppId,
                    Nome = jogo.Nome,
                    Imagem = jogo.Imagem,
                    PrecoOriginalCentavos = oferta.PrecoOriginalCentavos,
                    PrecoFinalCentavos = oferta.PrecoFinalCentavos,
                    PercentualDesconto = oferta.PercentualDesconto,
                    ConsultadoEm = oferta.ConsultadoEm
                });
            }
        }

        var ordenados = emPromocao
            .OrderByDescending(p => p.PercentualDesconto)
            .ThenBy(p => p.PrecoFinalCentavos ?? int.MaxValue)
            .ToList();

        return new PromocoesPretendeJogarResponseDto
        {
            NotificacoesAtivas = true,
            TotalEmPromocao = ordenados.Count,
            JogosEmPromocao = ordenados
        };
    }

    private RegraEstanteCatalogo ResolverRegraEstante(string tipoEstante)
    {
        if (string.IsNullOrWhiteSpace(tipoEstante))
        {
            throw new ValidationException("O tipo de estante é obrigatório.");
        }

        var normalizado = tipoEstante.Trim().ToLowerInvariant();
        var regra = regrasEstante.FirstOrDefault(r => r.TipoEstante.Equals(normalizado, StringComparison.OrdinalIgnoreCase));

        if (regra is null)
        {
            var disponiveis = string.Join(", ", regrasEstante.Select(r => r.TipoEstante));
            throw new ValidationException($"Tipo de estante '{tipoEstante}' inválido. Valores aceitos: {disponiveis}.");
        }

        return regra;
    }

    private async Task<Jogo> ObterOuSincronizarJogoAsync(int steamAppId, CancellationToken cancellationToken)
    {
        var existente = await catalogoRepository.ObterPorSteamAppIdAsync(steamAppId, cancellationToken);
        if (existente is not null)
        {
            return existente;
        }

        var detalhesSteam = await steamStoreService.ObterDetalhesJogoAsync(steamAppId, cancellationToken)
            ?? throw new NotFoundException($"Jogo com SteamAppId {steamAppId} não foi encontrado na Steam.");

        var oferta = CatalogoService.ExtrairOfertaTemporaria(detalhesSteam);
        await cacheService.SalvarAsync(CatalogoService.ObterChaveOferta(steamAppId), oferta, TtlOfertaTemporaria, cancellationToken);

        return await catalogoRepository.SalvarOuAtualizarAsync(detalhesSteam, cancellationToken);
    }

    private async Task<IReadOnlyDictionary<int, OfertaJogoSteamDto>> CarregarOfertasDoCacheAsync(
        IEnumerable<Jogo> jogos,
        CancellationToken cancellationToken)
    {
        var ofertas = new Dictionary<int, OfertaJogoSteamDto>();

        foreach (var jogo in jogos)
        {
            var oferta = await cacheService.ObterAsync<OfertaJogoSteamDto>(
                CatalogoService.ObterChaveOferta(jogo.SteamAppId),
                cancellationToken);

            if (oferta is not null)
            {
                ofertas[jogo.SteamAppId] = oferta;
            }
        }

        return ofertas;
    }

    private async Task PropagarImpactoNosLoggersAsync(
        int idUsuario,
        Jogo jogo,
        RegraEstanteCatalogo regra,
        bool adicionando,
        CancellationToken cancellationToken)
    {
        var jogoLogger = await loggerRepository.ObterOuCriarJogoLoggerAsync(jogo.IdJogo, cancellationToken);
        var usuarioLogger = await loggerRepository.ObterOuCriarUsuarioLoggerAsync(idUsuario, cancellationToken);

        var generoLoggers = new List<GeneroLogger>();
        foreach (var genero in jogo.Generos.Distinct())
        {
            var globalLogger = await loggerRepository.ObterOuCriarGeneroLoggerAsync(genero, null, cancellationToken);
            var individualLogger = await loggerRepository.ObterOuCriarGeneroLoggerAsync(genero, idUsuario, cancellationToken);

            generoLoggers.Add(globalLogger);
            generoLoggers.Add(individualLogger);
        }

        regra.AtualizarLoggers(jogoLogger, generoLoggers, usuarioLogger, adicionando);
        await loggerRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    private static Favoritos CriarFavoritosPadrao(int idUsuario) => new()
    {
        IdUsuario = idUsuario,
        Jogos = [],
        ExibirNoTopoPerfil = true,
        DataAtualizacao = DateTime.UtcNow
    };

    private static PretendeJogar CriarPretendeJogarPadrao(int idUsuario) => new()
    {
        IdUsuario = idUsuario,
        Jogos = [],
        NivelPrioridade = 1,
        NotificarEmPromocao = true,
        DataAtualizacao = DateTime.UtcNow
    };

    private static EstanteFavoritosResponseDto MapearFavoritos(
        Favoritos favoritos,
        IReadOnlyDictionary<int, Jogo> mapaJogos,
        IReadOnlyDictionary<int, JogoLogger> loggers,
        IReadOnlyDictionary<int, OfertaJogoSteamDto> ofertas)
    {
        var lista = MapearJogosDaEstante(favoritos.Jogos, mapaJogos, loggers, ofertas);

        return new EstanteFavoritosResponseDto
        {
            IdCatalogo = favoritos.IdCatalogo,
            ExibirNoTopoPerfil = favoritos.ExibirNoTopoPerfil,
            JogoDestaquePerfilId = favoritos.JogoDestaquePerfilId,
            ObservacaoDestaque = favoritos.ObservacaoDestaque,
            TotalJogos = lista.Count,
            DataAtualizacao = favoritos.DataAtualizacao,
            Jogos = lista
        };
    }

    private static EstantePretendeJogarResponseDto MapearPretendeJogar(
        PretendeJogar pretendeJogar,
        IReadOnlyDictionary<int, Jogo> mapaJogos,
        IReadOnlyDictionary<int, JogoLogger> loggers,
        IReadOnlyDictionary<int, OfertaJogoSteamDto> ofertas)
    {
        var lista = MapearJogosDaEstante(pretendeJogar.Jogos, mapaJogos, loggers, ofertas);

        return new EstantePretendeJogarResponseDto
        {
            IdCatalogo = pretendeJogar.IdCatalogo,
            NivelPrioridade = pretendeJogar.NivelPrioridade,
            NotificarEmPromocao = pretendeJogar.NotificarEmPromocao,
            MetaAnoConclusao = pretendeJogar.MetaAnoConclusao,
            TotalJogos = lista.Count,
            DataAtualizacao = pretendeJogar.DataAtualizacao,
            Jogos = lista
        };
    }

    private static IReadOnlyList<JogoResumoResponseDto> MapearJogosDaEstante(
        IEnumerable<int> idsJogos,
        IReadOnlyDictionary<int, Jogo> mapaJogos,
        IReadOnlyDictionary<int, JogoLogger> loggers,
        IReadOnlyDictionary<int, OfertaJogoSteamDto> ofertas)
    {
        var resultado = new List<JogoResumoResponseDto>();

        foreach (var idJogo in idsJogos)
        {
            if (mapaJogos.TryGetValue(idJogo, out var jogo))
            {
                loggers.TryGetValue(idJogo, out var logger);
                ofertas.TryGetValue(jogo.SteamAppId, out var oferta);
                var pontuacao = ContextoCuradoria.CalcularForcaJogoLogger(logger);
                var media = ContextoCuradoria.CalcularMediaFogueiras(logger);
                resultado.Add(CatalogoMapper.ToResumoResponse(jogo, pontuacao, media, oferta));
            }
        }

        return resultado;
    }
}

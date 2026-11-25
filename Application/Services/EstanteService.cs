using Application.Dtos.Catalogo.Responses;
using Application.Dtos.Estantes.Requests;
using Application.Dtos.Estantes.Responses;
using Application.Interfaces;
using Application.Mappers;
using Application.Strategies.Algoritmos;
using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Estantes.Commands;
using Domain.Dtos.Estantes.Respost;
using Domain.Dtos.Loggers.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;

namespace Application.Services;

public class EstanteService(
    IEstanteRepository estanteRepository,
    ICatalogoRepository catalogoRepository,
    IEngajamentoLoggerRepository loggerRepository,
    ISteamStoreService steamStoreService,
    ICatalogoCacheService cacheService,
    IRegraEstanteFactory regraEstanteFactory
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
        AlterarJogoEstanteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.SteamAppId <= 0)
        {
            throw new ValidationException("O identificador SteamAppId deve ser um número positivo.");
        }

        var regra = regraEstanteFactory.Criar(request.TipoEstante);
        var jogo = await ObterOuSincronizarJogoAsync(request.SteamAppId, cancellationToken);

        if (regra.TipoEstante == TipoEstante.Favoritos)
        {
            var favoritos = await estanteRepository.ObterFavoritosPorUsuarioAsync(idUsuario, cancellationToken)
                ?? CriarFavoritosPadrao(idUsuario);

            regra.ValidarAdicaoJogo(favoritos, jogo);

            var comando = new SalvarFavoritosCommandDto
            {
                IdUsuario = idUsuario,
                Jogos = [.. favoritos.Jogos, jogo.IdJogo],
                JogoDestaquePerfilId = favoritos.JogoDestaquePerfilId,
                ExibirNoTopoPerfil = favoritos.ExibirNoTopoPerfil,
                ObservacaoDestaque = favoritos.ObservacaoDestaque
            };

            await estanteRepository.SalvarFavoritosAsync(comando, cancellationToken);
        }
        else
        {
            var pretendeJogar = await estanteRepository.ObterPretendeJogarPorUsuarioAsync(idUsuario, cancellationToken)
                ?? CriarPretendeJogarPadrao(idUsuario);

            regra.ValidarAdicaoJogo(pretendeJogar, jogo);

            var comando = new SalvarPretendeJogarCommandDto
            {
                IdUsuario = idUsuario,
                Jogos = [.. pretendeJogar.Jogos, jogo.IdJogo],
                NivelPrioridade = pretendeJogar.NivelPrioridade,
                NotificarEmPromocao = pretendeJogar.NotificarEmPromocao,
                MetaAnoConclusao = pretendeJogar.MetaAnoConclusao
            };

            await estanteRepository.SalvarPretendeJogarAsync(comando, cancellationToken);
        }

        var impactoLoggers = regra.CriarComandoImpactoLoggers(idUsuario, jogo, adicionando: true);
        await loggerRepository.AplicarImpactoEstanteAsync(impactoLoggers, cancellationToken);

        return await ObterEstantesUsuarioAsync(idUsuario, cancellationToken);
    }

    public async Task<EstantesUsuarioResponseDto> RemoverJogoAsync(
        int idUsuario,
        AlterarJogoEstanteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.SteamAppId <= 0)
        {
            throw new ValidationException("O identificador SteamAppId deve ser um número positivo.");
        }

        var regra = regraEstanteFactory.Criar(request.TipoEstante);
        var jogo = await catalogoRepository.ObterPorSteamAppIdAsync(request.SteamAppId, cancellationToken)
            ?? throw new NotFoundException($"Jogo com SteamAppId {request.SteamAppId} não encontrado no catálogo.");

        if (regra.TipoEstante == TipoEstante.Favoritos)
        {
            var favoritos = await estanteRepository.ObterFavoritosPorUsuarioAsync(idUsuario, cancellationToken);
            if (favoritos is null || !favoritos.Jogos.Contains(jogo.IdJogo))
            {
                throw new NotFoundException("Este jogo não está na sua estante de Favoritos.");
            }

            var comando = new SalvarFavoritosCommandDto
            {
                IdUsuario = idUsuario,
                Jogos = favoritos.Jogos.Where(id => id != jogo.IdJogo).ToList(),
                JogoDestaquePerfilId = favoritos.JogoDestaquePerfilId == jogo.IdJogo ? null : favoritos.JogoDestaquePerfilId,
                ExibirNoTopoPerfil = favoritos.ExibirNoTopoPerfil,
                ObservacaoDestaque = favoritos.ObservacaoDestaque
            };

            await estanteRepository.SalvarFavoritosAsync(comando, cancellationToken);
        }
        else
        {
            var pretendeJogar = await estanteRepository.ObterPretendeJogarPorUsuarioAsync(idUsuario, cancellationToken);
            if (pretendeJogar is null || !pretendeJogar.Jogos.Contains(jogo.IdJogo))
            {
                throw new NotFoundException("Este jogo não está na sua estante 'Pretende Jogar'.");
            }

            var comando = new SalvarPretendeJogarCommandDto
            {
                IdUsuario = idUsuario,
                Jogos = pretendeJogar.Jogos.Where(id => id != jogo.IdJogo).ToList(),
                NivelPrioridade = pretendeJogar.NivelPrioridade,
                NotificarEmPromocao = pretendeJogar.NotificarEmPromocao,
                MetaAnoConclusao = pretendeJogar.MetaAnoConclusao
            };

            await estanteRepository.SalvarPretendeJogarAsync(comando, cancellationToken);
        }

        var impactoLoggers = regra.CriarComandoImpactoLoggers(idUsuario, jogo, adicionando: false);
        await loggerRepository.AplicarImpactoEstanteAsync(impactoLoggers, cancellationToken);

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

        var comando = new SalvarFavoritosCommandDto
        {
            IdUsuario = idUsuario,
            Jogos = favoritos.Jogos,
            JogoDestaquePerfilId = idJogoDestaque,
            ExibirNoTopoPerfil = request.ExibirNoTopoPerfil,
            ObservacaoDestaque = request.ObservacaoDestaque?.Trim()
        };

        var favoritosAtualizado = await estanteRepository.SalvarFavoritosAsync(comando, cancellationToken);

        var jogos = await catalogoRepository.ObterPorIdsAsync(favoritosAtualizado.Jogos, cancellationToken);
        var loggers = await loggerRepository.ObterJogoLoggersPorJogosAsync(favoritosAtualizado.Jogos, cancellationToken);
        var ofertas = await CarregarOfertasDoCacheAsync(jogos, cancellationToken);

        return MapearFavoritos(favoritosAtualizado, jogos.ToDictionary(j => j.IdJogo), loggers, ofertas);
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

        var comando = new SalvarPretendeJogarCommandDto
        {
            IdUsuario = idUsuario,
            Jogos = pretendeJogar.Jogos,
            NivelPrioridade = request.NivelPrioridade,
            NotificarEmPromocao = request.NotificarEmPromocao,
            MetaAnoConclusao = request.MetaAnoConclusao
        };

        var pretendeAtualizado = await estanteRepository.SalvarPretendeJogarAsync(comando, cancellationToken);

        var jogos = await catalogoRepository.ObterPorIdsAsync(pretendeAtualizado.Jogos, cancellationToken);
        var loggers = await loggerRepository.ObterJogoLoggersPorJogosAsync(pretendeAtualizado.Jogos, cancellationToken);
        var ofertas = await CarregarOfertasDoCacheAsync(jogos, cancellationToken);

        return MapearPretendeJogar(pretendeAtualizado, jogos.ToDictionary(j => j.IdJogo), loggers, ofertas);
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

    private async Task<JogoCatalogoRespostDto> ObterOuSincronizarJogoAsync(int steamAppId, CancellationToken cancellationToken)
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
        IEnumerable<JogoCatalogoRespostDto> jogos,
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

    private static FavoritosRespostDto CriarFavoritosPadrao(int idUsuario) => new()
    {
        IdUsuario = idUsuario,
        Jogos = [],
        ExibirNoTopoPerfil = true,
        DataAtualizacao = DateTime.UtcNow
    };

    private static PretendeJogarRespostDto CriarPretendeJogarPadrao(int idUsuario) => new()
    {
        IdUsuario = idUsuario,
        Jogos = [],
        NivelPrioridade = 1,
        NotificarEmPromocao = true,
        DataAtualizacao = DateTime.UtcNow
    };

    private static EstanteFavoritosResponseDto MapearFavoritos(
        FavoritosRespostDto favoritos,
        IReadOnlyDictionary<int, JogoCatalogoRespostDto> mapaJogos,
        IReadOnlyDictionary<int, JogoLoggerRespostDto> loggers,
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
        PretendeJogarRespostDto pretendeJogar,
        IReadOnlyDictionary<int, JogoCatalogoRespostDto> mapaJogos,
        IReadOnlyDictionary<int, JogoLoggerRespostDto> loggers,
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
        IReadOnlyDictionary<int, JogoCatalogoRespostDto> mapaJogos,
        IReadOnlyDictionary<int, JogoLoggerRespostDto> loggers,
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

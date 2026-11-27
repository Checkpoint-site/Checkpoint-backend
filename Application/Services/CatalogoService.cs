using Application.Dtos.Catalogo.Requests;
using Application.Dtos.Catalogo.Responses;
using Application.Interfaces;
using Application.Mappers;
using Application.Strategies.Algoritmos;
using Application.Utils;
using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Commands;
using Domain.Dtos.Catalogo.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;

namespace Application.Services;

public class CatalogoService(
    ICatalogoRepository catalogoRepository,
    IEngajamentoLoggerRepository loggerRepository,
    ISteamStoreService steamStoreService,
    ICatalogoCacheService cacheService,
    IBuscaCatalogoStrategyFactory buscaStrategyFactory,
    IAlgoritmoCuradoriaFactory algoritmoCuradoriaFactory,
    IPostRepository? postRepository = null
) : ICatalogoService
{
    private const int MinimoCandidatosLocais = 5;
    private const int MaximoHidratacoesSteamPorBusca = 5;
    private static readonly TimeSpan TtlBusca = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan TtlDestaques = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan TtlOfertaTemporaria = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan ValidadeMetadadosFixosSql = TimeSpan.FromDays(30);

    public static string ObterChaveOferta(int steamAppId) => $"catalogo:oferta:{steamAppId}";

    public async Task<BuscaCatalogoResponseDto> BuscarAsync(
        BuscarCatalogoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var estrategia = buscaStrategyFactory.Criar(request.Tipo);
        var algoritmoEscolhido = algoritmoCuradoriaFactory.Criar(request.Algoritmo);

        estrategia.ValidarBusca(request.JogoBuscado);

        var jogoBuscadoLimpo = request.JogoBuscado.Trim();
        var chaveCache = $"catalogo:busca:{estrategia.ChaveExibicao}:{jogoBuscadoLimpo.ToLowerInvariant()}:{algoritmoEscolhido.ChaveExibicao}";

        var emCache = await cacheService.ObterAsync<BuscaCatalogoResponseDto>(chaveCache, cancellationToken);
        if (emCache is not null)
        {
            return emCache;
        }

        var comandoBusca = new BuscarCandidatosLocaisCommandDto
        {
            TipoBusca = estrategia.TipoBusca,
            JogoBuscado = jogoBuscadoLimpo
        };

        var candidatosLocais = await catalogoRepository.BuscarCandidatosLocaisAsync(comandoBusca, cancellationToken);

        var todosCandidatos = candidatosLocais
            .GroupBy(j => j.SteamAppId)
            .Select(g => g.First())
            .ToDictionary(j => j.SteamAppId);

        var correspondenciasLocais = estrategia.FiltrarCandidatos(todosCandidatos.Values, jogoBuscadoLimpo);
        HashSet<int> steamIdsRetornadosPelaBusca = [];

        if (correspondenciasLocais.Count < MinimoCandidatosLocais)
        {
            steamIdsRetornadosPelaBusca = await HidratarCandidatosDaSteamAsync(
                jogoBuscadoLimpo,
                estrategia.TipoBusca,
                todosCandidatos,
                temFallbackLocal: correspondenciasLocais.Count > 0,
                cancellationToken);
        }

        var filtrados = estrategia.FiltrarCandidatos(todosCandidatos.Values, jogoBuscadoLimpo).ToList();

        if (estrategia.TipoBusca == TipoBuscaCatalogo.Nome && steamIdsRetornadosPelaBusca.Count > 0)
        {
            var idsJaIncluidos = filtrados.Select(f => f.Jogo.SteamAppId).ToHashSet();
            foreach (var steamId in steamIdsRetornadosPelaBusca)
            {
                if (!idsJaIncluidos.Contains(steamId) && todosCandidatos.TryGetValue(steamId, out var jogoHidratado))
                {
                    filtrados.Add((jogoHidratado, 0.6));
                }
            }
        }

        var jogosFiltrados = filtrados.Select(f => f.Jogo).ToList();
        var precisaOfertasAoVivo = algoritmoEscolhido.TipoAlgoritmo == TipoAlgoritmoCuradoria.CustoBeneficio;

        var contexto = await MontarContextoCuradoriaAsync(jogosFiltrados, precisaOfertasAoVivo, cancellationToken);
        var ordenados = algoritmoEscolhido.Ordenar(filtrados, contexto);

        var resposta = new BuscaCatalogoResponseDto
        {
            TipoBusca = estrategia.ChaveExibicao,
            JogoBuscado = jogoBuscadoLimpo,
            AlgoritmoAplicado = algoritmoEscolhido.ChaveExibicao,
            TotalResultados = ordenados.Count,
            Itens = ordenados.Select(CatalogoMapper.ToResumoResponse).ToList()
        };

        if (resposta.TotalResultados > 0)
        {
            await cacheService.SalvarAsync(chaveCache, resposta, TtlBusca, cancellationToken);
        }

        return resposta;
    }

    public async Task<DestaquesCatalogoResponseDto> ObterDestaquesAsync(
        ObterDestaquesCatalogoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var algoritmoEscolhido = algoritmoCuradoriaFactory.Criar(request.Algoritmo);
        const string chaveDestaquesBrutos = "catalogo:destaques:steam";

        var destaquesSteam = await cacheService.ObterAsync<DestaquesSteamDto>(chaveDestaquesBrutos, cancellationToken);
        if (destaquesSteam is null)
        {
            destaquesSteam = await steamStoreService.ObterDestaquesAsync(cancellationToken);
            await cacheService.SalvarAsync(chaveDestaquesBrutos, destaquesSteam, TtlDestaques, cancellationToken);

            var itensComPreco = destaquesSteam.MaisVendidos
                .Concat(destaquesSteam.Lancamentos)
                .Concat(destaquesSteam.OfertasEspeciais)
                .Concat(destaquesSteam.EmBreve)
                .GroupBy(i => i.SteamAppId)
                .Select(g => g.First());

            foreach (var item in itensComPreco)
            {
                var oferta = new OfertaJogoSteamDto
                {
                    SteamAppId = item.SteamAppId,
                    Gratuito = item.Gratuito,
                    PrecoOriginalCentavos = item.PrecoOriginalCentavos,
                    PrecoFinalCentavos = item.PrecoFinalCentavos,
                    PercentualDesconto = item.PercentualDesconto,
                    ConsultadoEm = DateTime.UtcNow
                };

                await cacheService.SalvarAsync(ObterChaveOferta(item.SteamAppId), oferta, TtlOfertaTemporaria, cancellationToken);
            }
        }

        var todosItensVitrine = destaquesSteam.MaisVendidos
            .Concat(destaquesSteam.Lancamentos)
            .Concat(destaquesSteam.OfertasEspeciais)
            .Concat(destaquesSteam.EmBreve)
            .ToList();

        var todosSteamIds = todosItensVitrine
            .Select(i => i.SteamAppId)
            .Distinct()
            .ToList();

        var jogosLocais = await catalogoRepository.ObterPorSteamAppIdsAsync(todosSteamIds, cancellationToken);
        var mapaPorSteamId = jogosLocais.ToDictionary(j => j.SteamAppId);

        var ofertasVitrine = todosItensVitrine
            .GroupBy(i => i.SteamAppId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var item = g.First();
                    return new OfertaJogoSteamDto
                    {
                        SteamAppId = item.SteamAppId,
                        Gratuito = item.Gratuito,
                        PrecoOriginalCentavos = item.PrecoOriginalCentavos,
                        PrecoFinalCentavos = item.PrecoFinalCentavos,
                        PercentualDesconto = item.PercentualDesconto
                    };
                });

        var contextoBase = await MontarContextoCuradoriaAsync(jogosLocais, hidratarOfertasExpiradas: false, cancellationToken);
        var ofertasMescladas = new Dictionary<int, OfertaJogoSteamDto>(ofertasVitrine);
        foreach (var kv in contextoBase.OfertasPorSteamAppId)
        {
            ofertasMescladas[kv.Key] = kv.Value;
        }

        var contexto = new ContextoCuradoria
        {
            JogoLoggersPorJogo = contextoBase.JogoLoggersPorJogo,
            GeneroLoggers = contextoBase.GeneroLoggers,
            OfertasPorSteamAppId = ofertasMescladas
        };

        return new DestaquesCatalogoResponseDto
        {
            AlgoritmoAplicado = algoritmoEscolhido.ChaveExibicao,
            MaisVendidos = OrdenarVitrine(destaquesSteam.MaisVendidos, mapaPorSteamId, algoritmoEscolhido, contexto),
            Lancamentos = OrdenarVitrine(destaquesSteam.Lancamentos, mapaPorSteamId, algoritmoEscolhido, contexto),
            OfertasEspeciais = OrdenarVitrine(destaquesSteam.OfertasEspeciais, mapaPorSteamId, algoritmoEscolhido, contexto),
            EmBreve = OrdenarVitrine(destaquesSteam.EmBreve, mapaPorSteamId, algoritmoEscolhido, contexto)
        };
    }

    public async Task<JogoDetalhesResponseDto> ObterDetalhesPorSteamAppIdAsync(
        ObterDetalhesJogoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.SteamAppId <= 0)
        {
            throw new ValidationException("O identificador SteamAppId deve ser um número positivo.");
        }

        var steamAppId = request.SteamAppId;
        var jogoLocal = await catalogoRepository.ObterPorSteamAppIdAsync(steamAppId, cancellationToken);
        var ofertaTemporaria = await cacheService.ObterAsync<OfertaJogoSteamDto>(ObterChaveOferta(steamAppId), cancellationToken);

        var precisaSincronizarFixos = jogoLocal is null
            || (DateTime.UtcNow - jogoLocal.SincronizadoEm) > ValidadeMetadadosFixosSql;
        var precisaAtualizarOferta = ofertaTemporaria is null;

        if (precisaSincronizarFixos || precisaAtualizarOferta)
        {
            try
            {
                var detalhesSteam = await steamStoreService.ObterDetalhesJogoAsync(steamAppId, cancellationToken);
                if (detalhesSteam is null)
                {
                    if (jogoLocal is null)
                    {
                        throw new NotFoundException($"Jogo com SteamAppId {steamAppId} não foi encontrado na Steam.");
                    }
                }
                else
                {
                    if (precisaSincronizarFixos || jogoLocal is null)
                    {
                        jogoLocal = await catalogoRepository.SalvarOuAtualizarAsync(detalhesSteam, cancellationToken);
                    }

                    ofertaTemporaria = ExtrairOfertaTemporaria(detalhesSteam);
                    await cacheService.SalvarAsync(ObterChaveOferta(steamAppId), ofertaTemporaria, TtlOfertaTemporaria, cancellationToken);
                }
            }
            catch (IntegrationException) when (jogoLocal is not null)
            {
                // Fallback resiliente: entrega os metadados fixos do SQL Server mesmo se a Steam oscilar.
            }
        }

        if (jogoLocal is null)
        {
            throw new NotFoundException($"Jogo com SteamAppId {steamAppId} não foi encontrado.");
        }

        var logger = await loggerRepository.ObterJogoLoggerAsync(jogoLocal.IdJogo, cancellationToken);
        var distribuicao = postRepository is not null
            ? await postRepository.ObterDistribuicaoFogueirasJogoAsync(jogoLocal.IdJogo, cancellationToken)
            : null;
        var relacionados = await ObterJogosRelacionadosInternoAsync(jogoLocal, cancellationToken);

        return CatalogoMapper.ToDetalhesResponse(jogoLocal, logger, ofertaTemporaria, distribuicao, relacionados);
    }

    public async Task<TendenciasGenerosResponseDto> ObterTendenciasGenerosAsync(
        CancellationToken cancellationToken = default)
    {
        var loggers = await loggerRepository.ObterGeneroLoggersAsync(cancellationToken);
        var mapaGeneros = loggers.ToDictionary(g => g.Genero);

        var itens = Enum.GetValues<Generos>()
            .Select(genero =>
            {
                mapaGeneros.TryGetValue(genero, out var logger);

                return new TendenciaGeneroItemResponseDto
                {
                    Genero = genero.ToString(),
                    Pontuacao = Math.Round(ContextoCuradoria.CalcularForcaGeneroLogger(logger), 2),
                    PresencaFavoritos = logger?.PresencaFavoritos ?? 0UL,
                    PresencaPretendeJogar = logger?.PresencaPretendeJogar ?? 0UL,
                    PresencaPost = logger?.PresencaPost ?? 0UL,
                    PresencaComentario = logger?.PresencaComentario ?? 0UL,
                    QuantidadeCurtida = logger?.QuantidadeCurtida ?? 0UL
                };
            })
            .OrderByDescending(i => i.Pontuacao)
            .ThenBy(i => i.Genero)
            .ToList();

        return new TendenciasGenerosResponseDto
        {
            Generos = itens
        };
    }

    public async Task<IReadOnlyList<UsuarioPopularResponseDto>> ObterUsuariosPopularesAsync(
        ObterUsuariosPopularesRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Limite <= 0 || request.Limite > 50)
        {
            throw new ValidationException("O limite deve estar entre 1 e 50 usuários.");
        }

        var populares = await loggerRepository.ObterUsuariosPopularesAsync(request.Limite, cancellationToken);

        return populares
            .Select(u =>
            {
                var pontuacao = ContextoCuradoria.CalcularForcaUsuarioLogger(
                    u.CurtidasRecebidas,
                    u.PresencaPost,
                    u.QuantidadeSeguidores,
                    u.PresencaComentario,
                    u.PresencaFavoritos,
                    u.PossuiFotoPerfil);

                return new UsuarioPopularResponseDto
                {
                    IdUsuario = u.IdUsuario,
                    NomeUsuario = u.NomeUsuario,
                    PontuacaoPopularidade = Math.Round(pontuacao, 2),
                    PresencaPost = u.PresencaPost,
                    PresencaComentario = u.PresencaComentario,
                    PresencaFavoritos = u.PresencaFavoritos,
                    CurtidasRecebidas = u.CurtidasRecebidas,
                    QuantidadeSeguidores = u.QuantidadeSeguidores,
                    PossuiFotoPerfil = u.PossuiFotoPerfil,
                    UltimosRelatos = u.UltimosRelatos
                };
            })
            .OrderByDescending(u => u.PontuacaoPopularidade)
            .ThenBy(u => u.NomeUsuario)
            .ToList();
    }

    public async Task<DescobertaCatalogoResponseDto> ObterJogosAleatoriosAsync(
        ObterJogosAleatoriosRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Limite <= 0 || request.Limite > 50)
        {
            throw new ValidationException("O limite deve estar entre 1 e 50 jogos.");
        }

        var algoritmoEscolhido = algoritmoCuradoriaFactory.Criar(request.Algoritmo);
        var jogosLocais = request.ForcarFallbackSteam
            ? []
            : (await catalogoRepository.ListarCatalogoLocalAsync(Math.Max(60, request.Limite * 2), cancellationToken)).ToList();

        var candidatos = new List<(JogoCatalogoRespostDto Jogo, double PesoRelevancia)>();
        var ofertasFallback = new Dictionary<int, OfertaJogoSteamDto>();
        string origemDados;

        if (!request.ForcarFallbackSteam && jogosLocais.Count >= request.Limite)
        {
            origemDados = "banco-local";
            candidatos.AddRange(jogosLocais.Select(j => (j, 1.0)));
        }
        else
        {
            var poolSteam = await ProvedorFallbackSteam.ObterPoolAsync(
                catalogoRepository,
                steamStoreService,
                cacheService,
                cancellationToken);

            foreach (var item in poolSteam)
            {
                if (item.Oferta is not null)
                {
                    ofertasFallback[item.Jogo.SteamAppId] = item.Oferta;
                }
            }

            if (jogosLocais.Count == 0)
            {
                origemDados = "steam-fallback";
                candidatos.AddRange(poolSteam.Select(p => (p.Jogo, p.PesoRelevanciaFonte)));
            }
            else
            {
                origemDados = poolSteam.Count > 0 ? "hibrido" : "banco-local";
                var steamIdsLocais = jogosLocais.Select(j => j.SteamAppId).ToHashSet();
                candidatos.AddRange(jogosLocais.Select(j => (j, 1.0)));
                candidatos.AddRange(
                    poolSteam
                        .Where(p => !steamIdsLocais.Contains(p.Jogo.SteamAppId))
                        .Select(p => (p.Jogo, p.PesoRelevanciaFonte)));
            }
        }

        var todosJogos = candidatos.Select(c => c.Jogo).ToList();
        var contextoBase = await MontarContextoCuradoriaAsync(
            todosJogos.Where(j => j.IdJogo > 0).ToList(),
            hidratarOfertasExpiradas: false,
            cancellationToken);

        var ofertasMescladas = new Dictionary<int, OfertaJogoSteamDto>(ofertasFallback);
        foreach (var kv in contextoBase.OfertasPorSteamAppId)
        {
            ofertasMescladas[kv.Key] = kv.Value;
        }

        var contexto = new ContextoCuradoria
        {
            JogoLoggersPorJogo = contextoBase.JogoLoggersPorJogo,
            GeneroLoggers = contextoBase.GeneroLoggers,
            OfertasPorSteamAppId = ofertasMescladas
        };

        var selecionados = ProvedorFallbackSteam
            .OrdenarComRelevanciaEAleatoriedade(candidatos, algoritmoEscolhido, contexto)
            .Take(request.Limite)
            .ToList();

        return new DescobertaCatalogoResponseDto
        {
            OrigemDados = origemDados,
            AlgoritmoAplicado = algoritmoEscolhido.ChaveExibicao,
            TotalResultados = selecionados.Count,
            Itens = selecionados
        };
    }

    internal static OfertaJogoSteamDto ExtrairOfertaTemporaria(DetalhesJogoSteamDto detalhes) => new()
    {
        SteamAppId = detalhes.SteamAppId,
        Gratuito = detalhes.Gratuito,
        PrecoOriginalCentavos = detalhes.PrecoOriginalCentavos,
        PrecoFinalCentavos = detalhes.PrecoFinalCentavos,
        PercentualDesconto = detalhes.PercentualDesconto,
        ConsultadoEm = DateTime.UtcNow
    };

    private async Task<HashSet<int>> HidratarCandidatosDaSteamAsync(
        string jogoBuscado,
        TipoBuscaCatalogo tipoBusca,
        Dictionary<int, JogoCatalogoRespostDto> todosCandidatos,
        bool temFallbackLocal,
        CancellationToken cancellationToken)
    {
        var idsEncontrados = new HashSet<int>();

        try
        {
            var itensSteam = await steamStoreService.BuscarJogosAsync(jogoBuscado, cancellationToken);
            foreach (var item in itensSteam)
            {
                idsEncontrados.Add(item.SteamAppId);
            }

            var idsParaHidratar = itensSteam
                .Where(i => !todosCandidatos.ContainsKey(i.SteamAppId))
                .Select(i => i.SteamAppId)
                .ToList();

            if (tipoBusca == TipoBuscaCatalogo.Ano && idsParaHidratar.Count < MaximoHidratacoesSteamPorBusca)
            {
                var destaques = await steamStoreService.ObterDestaquesAsync(cancellationToken);
                var idsVitrine = destaques.Lancamentos
                    .Concat(destaques.MaisVendidos)
                    .Select(i => i.SteamAppId)
                    .Where(id => !todosCandidatos.ContainsKey(id) && !idsParaHidratar.Contains(id));
                idsParaHidratar.AddRange(idsVitrine);
            }

            var loteIds = idsParaHidratar
                .Distinct()
                .Take(MaximoHidratacoesSteamPorBusca)
                .ToList();

            var tarefasDetalhes = loteIds
                .Select(appId => steamStoreService.ObterDetalhesJogoAsync(appId, cancellationToken))
                .ToList();

            var detalhesObtidos = await Task.WhenAll(tarefasDetalhes);

            foreach (var detalhes in detalhesObtidos)
            {
                if (detalhes is not null)
                {
                    var salvo = await catalogoRepository.SalvarOuAtualizarAsync(detalhes, cancellationToken);
                    todosCandidatos[salvo.SteamAppId] = salvo;

                    var oferta = ExtrairOfertaTemporaria(detalhes);
                    await cacheService.SalvarAsync(ObterChaveOferta(detalhes.SteamAppId), oferta, TtlOfertaTemporaria, cancellationToken);
                }
            }
        }
        catch (IntegrationException) when (temFallbackLocal)
        {
            // Se a Steam falhar mas já temos candidatos locais no SQL Server, retorna os locais com resiliência.
        }

        return idsEncontrados;
    }

    private async Task<ContextoCuradoria> MontarContextoCuradoriaAsync(
        IReadOnlyList<JogoCatalogoRespostDto> jogos,
        bool hidratarOfertasExpiradas,
        CancellationToken cancellationToken)
    {
        var idsJogos = jogos.Select(j => j.IdJogo).Distinct().ToList();
        var jogoLoggers = await loggerRepository.ObterJogoLoggersPorJogosAsync(idsJogos, cancellationToken);
        var generoLoggers = await loggerRepository.ObterGeneroLoggersAsync(cancellationToken);

        var ofertas = new Dictionary<int, OfertaJogoSteamDto>();
        var atualizacoesRealizadas = 0;

        foreach (var jogo in jogos)
        {
            var ofertaCache = await cacheService.ObterAsync<OfertaJogoSteamDto>(ObterChaveOferta(jogo.SteamAppId), cancellationToken);
            if (ofertaCache is not null)
            {
                ofertas[jogo.SteamAppId] = ofertaCache;
                continue;
            }

            if (hidratarOfertasExpiradas && !jogo.Gratuito && !jogo.EmBreve && atualizacoesRealizadas < MaximoHidratacoesSteamPorBusca)
            {
                try
                {
                    atualizacoesRealizadas++;
                    var detalhes = await steamStoreService.ObterDetalhesJogoAsync(jogo.SteamAppId, cancellationToken);
                    if (detalhes is not null)
                    {
                        var novaOferta = ExtrairOfertaTemporaria(detalhes);
                        await cacheService.SalvarAsync(ObterChaveOferta(jogo.SteamAppId), novaOferta, TtlOfertaTemporaria, cancellationToken);
                        ofertas[jogo.SteamAppId] = novaOferta;
                    }
                }
                catch (IntegrationException)
                {
                    // Continua com os demais itens se a Steam limitar requisições.
                }
            }
        }

        return new ContextoCuradoria
        {
            JogoLoggersPorJogo = jogoLoggers,
            GeneroLoggers = generoLoggers.ToDictionary(g => g.Genero),
            OfertasPorSteamAppId = ofertas
        };
    }

    private static IReadOnlyList<ItemVitrineResponseDto> OrdenarVitrine(
        IReadOnlyList<ItemDestaqueSteamDto> itens,
        IReadOnlyDictionary<int, JogoCatalogoRespostDto> mapaJogosLocais,
        AlgoritmoCuradoriaCatalogo algoritmo,
        ContextoCuradoria contexto)
    {
        return itens
            .Select(item =>
            {
                if (mapaJogosLocais.TryGetValue(item.SteamAppId, out var jogoLocal))
                {
                    var score = algoritmo.CalcularPontuacao(jogoLocal, 1.0, contexto);
                    var loggerLocal = contexto.ObterJogoLogger(jogoLocal.IdJogo);
                    return CatalogoMapper.ToItemVitrineResponse(item, score, jogoLocal, loggerLocal);
                }

                var jogoTemporario = new JogoCatalogoRespostDto
                {
                    IdJogo = 0,
                    SteamAppId = item.SteamAppId,
                    Nome = item.Nome,
                    Descricao = item.Nome,
                    Imagem = item.ImagemCapaUrl,
                    Desenvolvedora = "Steam",
                    Publicadora = "Steam",
                    Generos = [],
                    Gratuito = item.Gratuito
                };

                var scoreEstimado = algoritmo.CalcularPontuacao(jogoTemporario, 1.0, contexto);
                return CatalogoMapper.ToItemVitrineResponse(item, scoreEstimado);
            })
            .OrderByDescending(i => i.PontuacaoCuradoria)
            .ThenBy(i => i.Nome)
            .ToList();
    }

    private async Task<IReadOnlyList<JogoResumoResponseDto>> ObterJogosRelacionadosInternoAsync(
        JogoCatalogoRespostDto jogoBase,
        CancellationToken cancellationToken)
    {
        var candidatos = await catalogoRepository.ListarCatalogoLocalAsync(40, cancellationToken);
        var outros = candidatos.Where(j => j.IdJogo != jogoBase.IdJogo).ToList();
        if (outros.Count == 0)
        {
            return [];
        }

        var setGenerosBase = jogoBase.Generos.ToHashSet();
        var selecionados = outros
            .Select(j => new
            {
                Jogo = j,
                Afinidade = setGenerosBase.Count > 0
                    ? j.Generos.Count(g => setGenerosBase.Contains(g))
                    : 1
            })
            .Where(x => x.Afinidade > 0 || setGenerosBase.Count == 0)
            .OrderByDescending(x => x.Afinidade)
            .ThenByDescending(x => x.Jogo.MediaFogueiras)
            .ThenByDescending(x => x.Jogo.TotalAvaliacoes)
            .ThenBy(x => x.Jogo.Nome)
            .Take(6)
            .Select(x => x.Jogo)
            .ToList();

        if (selecionados.Count == 0)
        {
            selecionados = outros.Take(6).ToList();
        }

        var loggers = await loggerRepository.ObterJogoLoggersPorJogosAsync(selecionados.Select(j => j.IdJogo), cancellationToken);

        return selecionados
            .Select(j =>
            {
                loggers.TryGetValue(j.IdJogo, out var logger);
                var score = ContextoCuradoria.CalcularForcaJogoLogger(logger);
                var media = ContextoCuradoria.CalcularMediaFogueiras(logger);
                var total = logger is { TotalAvaliacoes: > 0 } ? (int)logger.TotalAvaliacoes : j.TotalAvaliacoes;
                return CatalogoMapper.ToResumoResponse(j, score, media, oferta: null, totalAvaliacoesOverride: total);
            })
            .ToList();
    }
}

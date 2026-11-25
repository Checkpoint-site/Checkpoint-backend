using Application.Dtos.Catalogo.Responses;
using Application.Interfaces;
using Application.Mappers;
using Application.Strategies.Algoritmos;
using Application.Strategies.Busca;
using Domain.Dtos.Catalogo;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Domain.Model;

namespace Application.Services;

public class CatalogoService(
    ICatalogoRepository catalogoRepository,
    IEngajamentoLoggerRepository loggerRepository,
    ISteamStoreService steamStoreService,
    ICatalogoCacheService cacheService,
    IEnumerable<EstrategiaBuscaCatalogo> estrategiasBusca,
    IEnumerable<AlgoritmoCuradoriaCatalogo> algoritmosCuradoria
) : ICatalogoService
{
    private const int MinimoCandidatosLocais = 5;
    private const int MaximoHidratacoesSteamPorBusca = 5;
    private static readonly TimeSpan TtlBuscaAnonima = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan TtlDestaques = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan TtlOfertaTemporaria = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan ValidadeMetadadosFixosSql = TimeSpan.FromDays(30);

    public static string ObterChaveOferta(int steamAppId) => $"catalogo:oferta:{steamAppId}";

    public async Task<BuscaCatalogoResponseDto> BuscarAsync(
        string? tipo,
        string termo,
        string? algoritmo,
        int? idUsuario = null,
        CancellationToken cancellationToken = default)
    {
        var estrategia = ResolverEstrategiaBusca(tipo);
        var algoritmoEscolhido = ResolverAlgoritmoCuradoria(algoritmo);

        estrategia.ValidarTermo(termo);

        var termoLimpo = termo.Trim();
        var chaveCache = $"catalogo:busca:{estrategia.TipoBusca}:{termoLimpo.ToLowerInvariant()}:{algoritmoEscolhido.NomeAlgoritmo}";

        if (idUsuario is null)
        {
            var emCache = await cacheService.ObterAsync<BuscaCatalogoResponseDto>(chaveCache, cancellationToken);
            if (emCache is not null)
            {
                return emCache;
            }
        }

        var candidatosLocais = await catalogoRepository.BuscarCandidatosLocaisAsync(
            estrategia.TipoBusca,
            termoLimpo,
            cancellationToken);

        var todosCandidatos = candidatosLocais
            .GroupBy(j => j.SteamAppId)
            .Select(g => g.First())
            .ToDictionary(j => j.SteamAppId);

        var correspondenciasLocais = estrategia.FiltrarCandidatos(todosCandidatos.Values, termoLimpo);

        if (correspondenciasLocais.Count < MinimoCandidatosLocais)
        {
            await HidratarCandidatosDaSteamAsync(termoLimpo, todosCandidatos, temFallbackLocal: correspondenciasLocais.Count > 0, cancellationToken);
        }

        var filtrados = estrategia.FiltrarCandidatos(todosCandidatos.Values, termoLimpo);
        var jogosFiltrados = filtrados.Select(f => f.Jogo).ToList();
        var precisaOfertasAoVivo = algoritmoEscolhido.NomeAlgoritmo == "custo-beneficio";

        var contexto = await MontarContextoCuradoriaAsync(jogosFiltrados, precisaOfertasAoVivo, idUsuario, cancellationToken);
        var ordenados = algoritmoEscolhido.Ordenar(filtrados, contexto);

        var resposta = new BuscaCatalogoResponseDto
        {
            TipoBusca = estrategia.TipoBusca,
            Termo = termoLimpo,
            AlgoritmoAplicado = algoritmoEscolhido.NomeAlgoritmo,
            PersonalizadoParaUsuario = idUsuario is not null,
            TotalResultados = ordenados.Count,
            Itens = ordenados.Select(CatalogoMapper.ToResumoResponse).ToList()
        };

        if (idUsuario is null && resposta.TotalResultados > 0)
        {
            await cacheService.SalvarAsync(chaveCache, resposta, TtlBuscaAnonima, cancellationToken);
        }

        return resposta;
    }

    public async Task<DestaquesCatalogoResponseDto> ObterDestaquesAsync(
        string? algoritmo = null,
        int? idUsuario = null,
        CancellationToken cancellationToken = default)
    {
        var algoritmoEscolhido = ResolverAlgoritmoCuradoria(algoritmo);
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

        var contextoBase = await MontarContextoCuradoriaAsync(jogosLocais, hidratarOfertasExpiradas: false, idUsuario, cancellationToken);
        var ofertasMescladas = new Dictionary<int, OfertaJogoSteamDto>(ofertasVitrine);
        foreach (var kv in contextoBase.OfertasPorSteamAppId)
        {
            ofertasMescladas[kv.Key] = kv.Value;
        }

        var contexto = new ContextoCuradoria
        {
            JogoLoggersPorJogo = contextoBase.JogoLoggersPorJogo,
            GeneroLoggersGlobais = contextoBase.GeneroLoggersGlobais,
            GeneroLoggersUsuario = contextoBase.GeneroLoggersUsuario,
            OfertasPorSteamAppId = ofertasMescladas,
            UsuarioAutenticado = contextoBase.UsuarioAutenticado
        };

        return new DestaquesCatalogoResponseDto
        {
            AlgoritmoAplicado = algoritmoEscolhido.NomeAlgoritmo,
            MaisVendidos = OrdenarVitrine(destaquesSteam.MaisVendidos, mapaPorSteamId, algoritmoEscolhido, contexto),
            Lancamentos = OrdenarVitrine(destaquesSteam.Lancamentos, mapaPorSteamId, algoritmoEscolhido, contexto),
            OfertasEspeciais = OrdenarVitrine(destaquesSteam.OfertasEspeciais, mapaPorSteamId, algoritmoEscolhido, contexto),
            EmBreve = OrdenarVitrine(destaquesSteam.EmBreve, mapaPorSteamId, algoritmoEscolhido, contexto)
        };
    }

    public async Task<JogoDetalhesResponseDto> ObterDetalhesPorSteamAppIdAsync(
        int steamAppId,
        CancellationToken cancellationToken = default)
    {
        if (steamAppId <= 0)
        {
            throw new ValidationException("O identificador SteamAppId deve ser um número positivo.");
        }

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
        return CatalogoMapper.ToDetalhesResponse(jogoLocal, logger, ofertaTemporaria);
    }

    public async Task<TendenciasGenerosResponseDto> ObterTendenciasGenerosAsync(
        int? idUsuario = null,
        CancellationToken cancellationToken = default)
    {
        var globais = await loggerRepository.ObterGeneroLoggersGlobaisAsync(cancellationToken);
        var mapaGlobal = globais.ToDictionary(g => g.Genero);

        Dictionary<Generos, GeneroLogger>? mapaUsuario = null;
        if (idUsuario is not null)
        {
            var doUsuario = await loggerRepository.ObterGeneroLoggersPorUsuarioAsync(idUsuario.Value, cancellationToken);
            mapaUsuario = doUsuario.ToDictionary(g => g.Genero);
        }

        var itens = Enum.GetValues<Generos>()
            .Select(genero =>
            {
                mapaGlobal.TryGetValue(genero, out var global);
                GeneroLogger? usuarioLogger = null;
                mapaUsuario?.TryGetValue(genero, out usuarioLogger);

                return new TendenciaGeneroItemResponseDto
                {
                    Genero = genero.ToString(),
                    PontuacaoGlobal = Math.Round(ContextoCuradoria.CalcularForcaGeneroLogger(global), 2),
                    PontuacaoUsuario = idUsuario is not null
                        ? Math.Round(ContextoCuradoria.CalcularForcaGeneroLogger(usuarioLogger), 2)
                        : null,
                    PresencaFavoritosGlobal = global?.PresencaFavoritos ?? 0UL,
                    PresencaPretendeJogarGlobal = global?.PresencaPretendeJogar ?? 0UL,
                    PresencaPostGlobal = global?.PresencaPost ?? 0UL,
                    PresencaComentarioGlobal = global?.PresencaComentario ?? 0UL,
                    QuantidadeCurtidaGlobal = global?.QuantidadeCurtida ?? 0UL
                };
            })
            .OrderByDescending(i => i.PontuacaoUsuario ?? i.PontuacaoGlobal)
            .ThenByDescending(i => i.PontuacaoGlobal)
            .ThenBy(i => i.Genero)
            .ToList();

        return new TendenciasGenerosResponseDto
        {
            PersonalizadoParaUsuario = idUsuario is not null,
            Generos = itens
        };
    }

    public async Task<IReadOnlyList<UsuarioPopularResponseDto>> ObterUsuariosPopularesAsync(
        int limite = 10,
        CancellationToken cancellationToken = default)
    {
        if (limite <= 0 || limite > 50)
        {
            throw new ValidationException("O limite deve estar entre 1 e 50 usuários.");
        }

        var populares = await loggerRepository.ObterUsuariosPopularesAsync(limite, cancellationToken);

        return populares
            .Select(u =>
            {
                var pontuacao = (u.CurtidasRecebidas * 4.0)
                              + (u.PresencaPost * 5.0)
                              + (u.QuantidadeSeguidores * 3.0)
                              + (u.PresencaComentario * 2.0)
                              + (u.PresencaFavoritos * 1.0);

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
                    UltimosRelatos = u.UltimosRelatos
                };
            })
            .ToList();
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

    private EstrategiaBuscaCatalogo ResolverEstrategiaBusca(string? tipo)
    {
        var chave = string.IsNullOrWhiteSpace(tipo) ? "nome" : tipo.Trim().ToLowerInvariant();
        var estrategia = estrategiasBusca.FirstOrDefault(e => e.TipoBusca.Equals(chave, StringComparison.OrdinalIgnoreCase));

        if (estrategia is null)
        {
            var disponiveis = string.Join(", ", estrategiasBusca.Select(e => e.TipoBusca));
            throw new ValidationException($"Tipo de busca '{tipo}' inválido. Valores aceitos: {disponiveis}.");
        }

        return estrategia;
    }

    private AlgoritmoCuradoriaCatalogo ResolverAlgoritmoCuradoria(string? algoritmo)
    {
        var chave = string.IsNullOrWhiteSpace(algoritmo) ? "popularidade" : algoritmo.Trim().ToLowerInvariant();
        var escolhido = algoritmosCuradoria.FirstOrDefault(a => a.NomeAlgoritmo.Equals(chave, StringComparison.OrdinalIgnoreCase));

        if (escolhido is null)
        {
            var disponiveis = string.Join(", ", algoritmosCuradoria.Select(a => a.NomeAlgoritmo));
            throw new ValidationException($"Algoritmo '{algoritmo}' inválido. Valores aceitos: {disponiveis}.");
        }

        return escolhido;
    }

    private async Task HidratarCandidatosDaSteamAsync(
        string termo,
        Dictionary<int, Jogo> todosCandidatos,
        bool temFallbackLocal,
        CancellationToken cancellationToken)
    {
        try
        {
            var itensSteam = await steamStoreService.BuscarPorTermoAsync(termo, cancellationToken);
            var ineditos = itensSteam
                .Where(i => !todosCandidatos.ContainsKey(i.SteamAppId))
                .Take(MaximoHidratacoesSteamPorBusca)
                .ToList();

            foreach (var item in ineditos)
            {
                var detalhes = await steamStoreService.ObterDetalhesJogoAsync(item.SteamAppId, cancellationToken);
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
    }

    private async Task<ContextoCuradoria> MontarContextoCuradoriaAsync(
        IReadOnlyList<Jogo> jogos,
        bool hidratarOfertasExpiradas,
        int? idUsuario,
        CancellationToken cancellationToken)
    {
        var idsJogos = jogos.Select(j => j.IdJogo).Distinct().ToList();
        var jogoLoggers = await loggerRepository.ObterJogoLoggersPorJogosAsync(idsJogos, cancellationToken);
        var globais = await loggerRepository.ObterGeneroLoggersGlobaisAsync(cancellationToken);

        IReadOnlyDictionary<Generos, GeneroLogger> doUsuario = new Dictionary<Generos, GeneroLogger>();
        if (idUsuario is not null)
        {
            var listaUsuario = await loggerRepository.ObterGeneroLoggersPorUsuarioAsync(idUsuario.Value, cancellationToken);
            doUsuario = listaUsuario.ToDictionary(g => g.Genero);
        }

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
            GeneroLoggersGlobais = globais.ToDictionary(g => g.Genero),
            GeneroLoggersUsuario = doUsuario,
            OfertasPorSteamAppId = ofertas,
            UsuarioAutenticado = idUsuario is not null
        };
    }

    private static IReadOnlyList<ItemVitrineResponseDto> OrdenarVitrine(
        IReadOnlyList<ItemDestaqueSteamDto> itens,
        IReadOnlyDictionary<int, Jogo> mapaJogosLocais,
        AlgoritmoCuradoriaCatalogo algoritmo,
        ContextoCuradoria contexto)
    {
        return itens
            .Select(item =>
            {
                if (mapaJogosLocais.TryGetValue(item.SteamAppId, out var jogoLocal))
                {
                    var score = algoritmo.CalcularPontuacao(jogoLocal, 1.0, contexto);
                    return CatalogoMapper.ToItemVitrineResponse(item, score);
                }

                var jogoTemporario = new Jogo
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
}

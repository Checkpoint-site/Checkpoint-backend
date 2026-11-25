using Application.Dtos.Estantes.Requests;
using Application.Services;
using Application.Strategies.Algoritmos;
using Application.Strategies.Busca;
using Application.Strategies.Estantes;
using Domain.Dtos.Catalogo;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Domain.Model;

namespace Test.Application.Services;

public class CatalogoEEstanteServiceTests
{
    private sealed class FakeCatalogoRepository : ICatalogoRepository
    {
        public readonly List<Jogo> Jogos = [];
        private int _nextId = 1;

        public Task<Jogo?> ObterPorIdAsync(int idJogo, CancellationToken ct = default) =>
            Task.FromResult(Jogos.FirstOrDefault(j => j.IdJogo == idJogo));

        public Task<IReadOnlyList<Jogo>> ObterPorIdsAsync(IEnumerable<int> idsJogos, CancellationToken ct = default)
        {
            var set = idsJogos.ToHashSet();
            return Task.FromResult<IReadOnlyList<Jogo>>(Jogos.Where(j => set.Contains(j.IdJogo)).ToList());
        }

        public Task<Jogo?> ObterPorSteamAppIdAsync(int steamAppId, CancellationToken ct = default) =>
            Task.FromResult(Jogos.FirstOrDefault(j => j.SteamAppId == steamAppId));

        public Task<IReadOnlyList<Jogo>> ObterPorSteamAppIdsAsync(IEnumerable<int> steamAppIds, CancellationToken ct = default)
        {
            var set = steamAppIds.ToHashSet();
            return Task.FromResult<IReadOnlyList<Jogo>>(Jogos.Where(j => set.Contains(j.SteamAppId)).ToList());
        }

        public Task<IReadOnlyList<Jogo>> BuscarCandidatosLocaisAsync(string tipoBusca, string termo, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Jogo>>(Jogos.ToList());

        public Task<Jogo> SalvarOuAtualizarAsync(DetalhesJogoSteamDto d, CancellationToken ct = default)
        {
            var existente = Jogos.FirstOrDefault(j => j.SteamAppId == d.SteamAppId);
            if (existente is not null)
            {
                existente.Nome = d.Nome;
                existente.SincronizadoEm = DateTime.UtcNow;
                return Task.FromResult(existente);
            }

            var novo = new Jogo
            {
                IdJogo = _nextId++,
                SteamAppId = d.SteamAppId,
                Nome = d.Nome,
                Descricao = d.DescricaoCurta,
                Imagem = d.ImagemCapaUrl,
                Desenvolvedora = d.Desenvolvedora,
                Publicadora = d.Publicadora,
                Generos = d.Generos.ToList(),
                DataLancamentoTexto = d.DataLancamentoTexto,
                AnoLancamento = d.AnoLancamento,
                NotaMetacritic = d.NotaMetacritic,
                TotalRecomendacoes = d.TotalRecomendacoes,
                Gratuito = d.Gratuito,
                EmBreve = d.EmBreve,
                SincronizadoEm = DateTime.UtcNow
            };

            Jogos.Add(novo);
            return Task.FromResult(novo);
        }
    }

    private sealed class FakeEstanteRepository : IEstanteRepository
    {
        public readonly Dictionary<int, Favoritos> FavoritosPorUsuario = [];
        public readonly Dictionary<int, PretendeJogar> PretendePorUsuario = [];
        private int _nextId = 1;

        public Task<Favoritos?> ObterFavoritosPorUsuarioAsync(int idUsuario, CancellationToken ct = default) =>
            Task.FromResult(FavoritosPorUsuario.TryGetValue(idUsuario, out var fav) ? fav : null);

        public Task<PretendeJogar?> ObterPretendeJogarPorUsuarioAsync(int idUsuario, CancellationToken ct = default) =>
            Task.FromResult(PretendePorUsuario.TryGetValue(idUsuario, out var pj) ? pj : null);

        public Task<Favoritos> SalvarFavoritosAsync(Favoritos favoritos, CancellationToken ct = default)
        {
            if (favoritos.IdCatalogo == 0)
            {
                favoritos.IdCatalogo = _nextId++;
            }

            FavoritosPorUsuario[favoritos.IdUsuario] = favoritos;
            return Task.FromResult(favoritos);
        }

        public Task<PretendeJogar> SalvarPretendeJogarAsync(PretendeJogar pretendeJogar, CancellationToken ct = default)
        {
            if (pretendeJogar.IdCatalogo == 0)
            {
                pretendeJogar.IdCatalogo = _nextId++;
            }

            PretendePorUsuario[pretendeJogar.IdUsuario] = pretendeJogar;
            return Task.FromResult(pretendeJogar);
        }
    }

    private sealed class FakeLoggerRepository : IEngajamentoLoggerRepository
    {
        public readonly List<GeneroLogger> GeneroLoggers = [];
        public readonly Dictionary<int, JogoLogger> JogoLoggers = [];
        public readonly Dictionary<int, UsuarioLogger> UsuarioLoggers = [];
        public List<UsuarioPopularComunidadeDto> Populares = [];

        public Task<IReadOnlyList<GeneroLogger>> ObterGeneroLoggersGlobaisAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<GeneroLogger>>(GeneroLoggers.Where(g => g.IdUsuario == null).ToList());

        public Task<IReadOnlyList<GeneroLogger>> ObterGeneroLoggersPorUsuarioAsync(int idUsuario, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<GeneroLogger>>(GeneroLoggers.Where(g => g.IdUsuario == idUsuario).ToList());

        public Task<GeneroLogger> ObterOuCriarGeneroLoggerAsync(Generos genero, int? idUsuario, CancellationToken ct = default)
        {
            var existente = GeneroLoggers.FirstOrDefault(g => g.Genero == genero && g.IdUsuario == idUsuario);
            if (existente is not null)
            {
                return Task.FromResult(existente);
            }

            var novo = new GeneroLogger { Genero = genero, IdUsuario = idUsuario };
            GeneroLoggers.Add(novo);
            return Task.FromResult(novo);
        }

        public Task<JogoLogger?> ObterJogoLoggerAsync(int idJogo, CancellationToken ct = default) =>
            Task.FromResult(JogoLoggers.TryGetValue(idJogo, out var l) ? l : null);

        public Task<IReadOnlyDictionary<int, JogoLogger>> ObterJogoLoggersPorJogosAsync(IEnumerable<int> idsJogos, CancellationToken ct = default)
        {
            var set = idsJogos.ToHashSet();
            IReadOnlyDictionary<int, JogoLogger> dict = JogoLoggers
                .Where(kv => set.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            return Task.FromResult(dict);
        }

        public Task<JogoLogger> ObterOuCriarJogoLoggerAsync(int idJogo, CancellationToken ct = default)
        {
            if (!JogoLoggers.TryGetValue(idJogo, out var l))
            {
                l = new JogoLogger { IdJogo = idJogo };
                JogoLoggers[idJogo] = l;
            }

            return Task.FromResult(l);
        }

        public Task<UsuarioLogger> ObterOuCriarUsuarioLoggerAsync(int idUsuario, CancellationToken ct = default)
        {
            if (!UsuarioLoggers.TryGetValue(idUsuario, out var u))
            {
                u = new UsuarioLogger { IdUsuario = idUsuario };
                UsuarioLoggers[idUsuario] = u;
            }

            return Task.FromResult(u);
        }

        public Task<IReadOnlyList<UsuarioPopularComunidadeDto>> ObterUsuariosPopularesAsync(int limite = 10, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<UsuarioPopularComunidadeDto>>(Populares.Take(limite).ToList());

        public Task SalvarAlteracoesAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeSteamStoreService : ISteamStoreService
    {
        public bool FalharComunicacao { get; set; }
        public readonly Dictionary<int, DetalhesJogoSteamDto> Detalhes = [];

        public Task<IReadOnlyList<ItemBuscaSteamDto>> BuscarPorTermoAsync(string termo, CancellationToken ct = default)
        {
            if (FalharComunicacao)
            {
                throw new IntegrationException("Steam indisponível.");
            }

            var itens = Detalhes.Values
                .Select(d => new ItemBuscaSteamDto
                {
                    SteamAppId = d.SteamAppId,
                    Nome = d.Nome,
                    ImagemCapaUrl = d.ImagemCapaUrl,
                    Gratuito = d.Gratuito,
                    PrecoOriginalCentavos = d.PrecoOriginalCentavos,
                    PrecoFinalCentavos = d.PrecoFinalCentavos
                })
                .ToList();

            return Task.FromResult<IReadOnlyList<ItemBuscaSteamDto>>(itens);
        }

        public Task<DestaquesSteamDto> ObterDestaquesAsync(CancellationToken ct = default) =>
            Task.FromResult(new DestaquesSteamDto
            {
                MaisVendidos = [],
                Lancamentos = [],
                OfertasEspeciais = [],
                EmBreve = []
            });

        public Task<DetalhesJogoSteamDto?> ObterDetalhesJogoAsync(int steamAppId, CancellationToken ct = default)
        {
            if (FalharComunicacao)
            {
                throw new IntegrationException("Steam indisponível.");
            }

            return Task.FromResult(Detalhes.TryGetValue(steamAppId, out var d) ? d : null);
        }
    }

    private sealed class FakeCacheService : ICatalogoCacheService
    {
        public readonly Dictionary<string, object> Dados = [];

        public Task<T?> ObterAsync<T>(string chave, CancellationToken ct = default) =>
            Task.FromResult(Dados.TryGetValue(chave, out var valor) && valor is T tipado ? tipado : default);

        public Task SalvarAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken ct = default)
        {
            if (valor is not null)
            {
                Dados[chave] = valor;
            }

            return Task.CompletedTask;
        }

        public Task RemoverAsync(string chave, CancellationToken ct = default)
        {
            Dados.Remove(chave);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task FluxoIntegrado_AdicionarAosFavoritos_SincronizaSteamAtualizaTresLoggersERanqueiaBusca()
    {
        var catalogoRepo = new FakeCatalogoRepository();
        var estanteRepo = new FakeEstanteRepository();
        var loggerRepo = new FakeLoggerRepository();
        var steamService = new FakeSteamStoreService();
        var cacheService = new FakeCacheService();

        steamService.Detalhes[292030] = new DetalhesJogoSteamDto
        {
            SteamAppId = 292030,
            Nome = "The Witcher 3: Wild Hunt",
            DescricaoCurta = "RPG de mundo aberto.",
            ImagemCapaUrl = "https://steam/292030.jpg",
            Desenvolvedora = "CD PROJEKT RED",
            Publicadora = "CD PROJEKT RED",
            Generos = [Generos.Rpg, Generos.Aventura],
            DataLancamentoTexto = "18 mai. 2015",
            AnoLancamento = 2015,
            NotaMetacritic = 93,
            TotalRecomendacoes = 700000,
            PrecoOriginalCentavos = 12999,
            PrecoFinalCentavos = 3249,
            PercentualDesconto = 75
        };

        var estanteService = new EstanteService(
            estanteRepo,
            catalogoRepo,
            loggerRepo,
            steamService,
            cacheService,
            [new RegraEstanteFavoritos(), new RegraEstantePretendeJogar()]);

        var catalogoService = new CatalogoService(
            catalogoRepo,
            loggerRepo,
            steamService,
            cacheService,
            [new BuscaPorNomeStrategy(), new BuscaPorAnoStrategy(), new BuscaPorAutorStrategy()],
            [new AlgoritmoPopularidadeJogo(), new AlgoritmoTendenciaGenero(), new AlgoritmoCustoBeneficio()]);

        var estantes = await estanteService.AdicionarJogoAsync(idUsuario: 42, tipoEstante: "favoritos", steamAppId: 292030);
        Assert.Single(estantes.Favoritos.Jogos);
        Assert.Equal(75, estantes.Favoritos.Jogos[0].PercentualDesconto);

        var favAtualizado = await estanteService.AtualizarConfiguracaoFavoritosAsync(42, new AtualizarFavoritosConfigRequestDto
        {
            JogoDestaqueSteamAppId = 292030,
            ExibirNoTopoPerfil = true,
            ObservacaoDestaque = "Obra-prima absoluta!"
        });
        Assert.Equal("Obra-prima absoluta!", favAtualizado.ObservacaoDestaque);
        Assert.NotNull(favAtualizado.JogoDestaquePerfilId);

        var jogoSalvo = catalogoRepo.Jogos.Single();
        Assert.Equal(1UL, loggerRepo.JogoLoggers[jogoSalvo.IdJogo].PresencaFavoritos);
        Assert.Equal(1UL, loggerRepo.UsuarioLoggers[42].PresencaFavoritos);

        var tendencias = await catalogoService.ObterTendenciasGenerosAsync(idUsuario: 42);
        Assert.True(tendencias.PersonalizadoParaUsuario);
        var topDois = tendencias.Generos.Take(2).ToList();
        Assert.Contains(topDois, g => g.Genero == "Rpg" && g.PontuacaoGlobal > 0);
        Assert.Contains(topDois, g => g.Genero == "Aventura" && g.PontuacaoGlobal > 0);

        var buscaPorAutor = await catalogoService.BuscarAsync("autor", "CD PROJEKT", "popularidade", idUsuario: null);
        Assert.Equal(1, buscaPorAutor.TotalResultados);
        Assert.Equal("The Witcher 3: Wild Hunt", buscaPorAutor.Itens[0].Nome);
    }

    [Fact]
    public async Task ObterPromocoesPretendeJogar_QuandoCacheOfertaExpira_ConsultaPrecoAoVivoNaSteamSemUsarBancoFixo()
    {
        var catalogoRepo = new FakeCatalogoRepository();
        var estanteRepo = new FakeEstanteRepository();
        var loggerRepo = new FakeLoggerRepository();
        var steamService = new FakeSteamStoreService();
        var cacheService = new FakeCacheService();

        steamService.Detalhes[1091500] = new DetalhesJogoSteamDto
        {
            SteamAppId = 1091500,
            Nome = "Cyberpunk 2077",
            DescricaoCurta = "RPG futurista",
            ImagemCapaUrl = "https://steam/cyberpunk.jpg",
            Desenvolvedora = "CD PROJEKT RED",
            Publicadora = "CD PROJEKT RED",
            Generos = [Generos.Rpg, Generos.Acao],
            AnoLancamento = 2020,
            PrecoOriginalCentavos = 19990,
            PrecoFinalCentavos = 19990,
            PercentualDesconto = 0
        };

        var estanteService = new EstanteService(
            estanteRepo,
            catalogoRepo,
            loggerRepo,
            steamService,
            cacheService,
            [new RegraEstanteFavoritos(), new RegraEstantePretendeJogar()]);

        // 1. Usuário adiciona sem promoção no momento
        await estanteService.AdicionarJogoAsync(idUsuario: 15, tipoEstante: "pretende-jogar", steamAppId: 1091500);

        // 2. Cache temporário do Redis expira (30 min) e entra promoção de 60% na Steam
        cacheService.Dados.Clear();
        steamService.Detalhes[1091500] = steamService.Detalhes[1091500] with
        {
            PrecoFinalCentavos = 7996,
            PercentualDesconto = 60
        };

        // 3. Verificação de promoções detecta a oferta nova ao vivo e repopula o Redis
        var promocoes = await estanteService.ObterPromocoesPretendeJogarAsync(idUsuario: 15);

        Assert.True(promocoes.NotificacoesAtivas);
        Assert.Equal(1, promocoes.TotalEmPromocao);
        Assert.Equal("Cyberpunk 2077", promocoes.JogosEmPromocao[0].Nome);
        Assert.Equal(60, promocoes.JogosEmPromocao[0].PercentualDesconto);
        Assert.Equal(7996, promocoes.JogosEmPromocao[0].PrecoFinalCentavos);
    }

    [Fact]
    public async Task ObterDetalhesPorSteamAppId_QuandoSteamFalhaMasExisteNoSql_RetornaFallbackLocal()
    {
        var catalogoRepo = new FakeCatalogoRepository();
        var loggerRepo = new FakeLoggerRepository();
        var steamService = new FakeSteamStoreService { FalharComunicacao = true };
        var cacheService = new FakeCacheService();

        catalogoRepo.Jogos.Add(new Jogo
        {
            IdJogo = 7,
            SteamAppId = 1245620,
            Nome = "ELDEN RING",
            Descricao = "Action RPG",
            Imagem = "https://steam/elden.jpg",
            Desenvolvedora = "FromSoftware",
            Publicadora = "Bandai Namco",
            Generos = [Generos.Rpg, Generos.Acao],
            AnoLancamento = 2022,
            SincronizadoEm = DateTime.UtcNow.AddDays(-45)
        });

        var catalogoService = new CatalogoService(
            catalogoRepo,
            loggerRepo,
            steamService,
            cacheService,
            [new BuscaPorNomeStrategy()],
            [new AlgoritmoPopularidadeJogo()]);

        var detalhes = await catalogoService.ObterDetalhesPorSteamAppIdAsync(1245620);

        Assert.Equal("ELDEN RING", detalhes.Nome);
        Assert.Equal(2022, detalhes.AnoLancamento);
    }
}

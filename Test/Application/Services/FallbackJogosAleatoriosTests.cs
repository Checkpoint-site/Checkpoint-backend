using Application.Dtos.Catalogo.Requests;
using Application.Factories;
using Application.Services;
using Application.Strategies.Algoritmos;
using Application.Strategies.Busca;
using Application.Utils;
using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Commands;
using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Loggers.Commands;
using Domain.Dtos.Loggers.Respost;
using Domain.Dtos.Posts.Commands;
using Domain.Dtos.Posts.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;

namespace Test.Application.Services;

public class FallbackJogosAleatoriosTests
{
    private sealed class FakeCatalogoRepo : ICatalogoRepository
    {
        public readonly List<JogoCatalogoRespostDto> Jogos = [];
        public int ChamadasSalvar { get; private set; }

        public Task<JogoCatalogoRespostDto?> ObterPorIdAsync(int idJogo, CancellationToken ct = default) =>
            Task.FromResult(Jogos.FirstOrDefault(j => j.IdJogo == idJogo));

        public Task<IReadOnlyList<JogoCatalogoRespostDto>> ObterPorIdsAsync(IEnumerable<int> idsJogos, CancellationToken ct = default)
        {
            var set = idsJogos.ToHashSet();
            return Task.FromResult<IReadOnlyList<JogoCatalogoRespostDto>>(Jogos.Where(j => set.Contains(j.IdJogo)).ToList());
        }

        public Task<JogoCatalogoRespostDto?> ObterPorSteamAppIdAsync(int steamAppId, CancellationToken ct = default) =>
            Task.FromResult(Jogos.FirstOrDefault(j => j.SteamAppId == steamAppId));

        public Task<IReadOnlyList<JogoCatalogoRespostDto>> ObterPorSteamAppIdsAsync(IEnumerable<int> steamAppIds, CancellationToken ct = default)
        {
            var set = steamAppIds.ToHashSet();
            return Task.FromResult<IReadOnlyList<JogoCatalogoRespostDto>>(Jogos.Where(j => set.Contains(j.SteamAppId)).ToList());
        }

        public Task<IReadOnlyList<JogoCatalogoRespostDto>> BuscarCandidatosLocaisAsync(BuscarCandidatosLocaisCommandDto command, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<JogoCatalogoRespostDto>>(Jogos.ToList());

        public Task<IReadOnlyList<JogoCatalogoRespostDto>> ListarCatalogoLocalAsync(int limite = 60, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<JogoCatalogoRespostDto>>(Jogos.Take(limite).ToList());

        public Task<JogoCatalogoRespostDto> SalvarOuAtualizarAsync(DetalhesJogoSteamDto detalhesSteam, CancellationToken ct = default)
        {
            ChamadasSalvar++;
            var novo = new JogoCatalogoRespostDto
            {
                IdJogo = Jogos.Count + 1,
                SteamAppId = detalhesSteam.SteamAppId,
                Nome = detalhesSteam.Nome,
                Descricao = detalhesSteam.DescricaoCurta,
                Imagem = detalhesSteam.ImagemCapaUrl,
                Desenvolvedora = detalhesSteam.Desenvolvedora,
                Publicadora = detalhesSteam.Publicadora,
                Generos = detalhesSteam.Generos.ToList(),
                AnoLancamento = detalhesSteam.AnoLancamento,
                NotaMetacritic = detalhesSteam.NotaMetacritic,
                TotalRecomendacoes = detalhesSteam.TotalRecomendacoes,
                Gratuito = detalhesSteam.Gratuito,
                EmBreve = detalhesSteam.EmBreve
            };
            Jogos.Add(novo);
            return Task.FromResult(novo);
        }
    }

    private sealed class FakeLoggerRepo : IEngajamentoLoggerRepository
    {
        public readonly Dictionary<Generos, GeneroLoggerRespostDto> Generos = [];
        public readonly Dictionary<int, JogoLoggerRespostDto> Jogos = [];

        public Task<IReadOnlyList<GeneroLoggerRespostDto>> ObterGeneroLoggersAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<GeneroLoggerRespostDto>>(Generos.Values.ToList());

        public Task<JogoLoggerRespostDto?> ObterJogoLoggerAsync(int idJogo, CancellationToken ct = default) =>
            Task.FromResult(Jogos.TryGetValue(idJogo, out var l) ? l : null);

        public Task<IReadOnlyDictionary<int, JogoLoggerRespostDto>> ObterJogoLoggersPorJogosAsync(IEnumerable<int> idsJogos, CancellationToken ct = default)
        {
            var set = idsJogos.ToHashSet();
            IReadOnlyDictionary<int, JogoLoggerRespostDto> dict = Jogos
                .Where(kv => set.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            return Task.FromResult(dict);
        }

        public Task AplicarImpactoEstanteAsync(ImpactoEngajamentoEstanteCommandDto command, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task SincronizarAvaliacoesJogoEUsuarioAsync(int idJogo, int idUsuario, IReadOnlyList<Generos> generosJogo, int deltaPostsGenero = 0, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<UsuarioPopularComunidadeDto>> ObterUsuariosPopularesAsync(int limite = 10, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<UsuarioPopularComunidadeDto>>([]);
    }

    private sealed class FakePostRepo : IPostRepository
    {
        public Task<PostDetalheRespostDto?> ObterPorIdAsync(int idPost, int? idUsuarioAtual = null, CancellationToken ct = default) =>
            Task.FromResult<PostDetalheRespostDto?>(null);

        public Task<IReadOnlyList<PostDetalheRespostDto>> ListarRecentesAsync(int limite = 20, int? idUsuarioAtual = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PostDetalheRespostDto>>([]);

        public Task<IReadOnlyList<PostDetalheRespostDto>> ListarPorJogoAsync(int idJogo, string? ordenacao = null, int? idUsuarioAtual = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PostDetalheRespostDto>>([]);

        public Task<IReadOnlyList<PostDetalheRespostDto>> ListarPorUsuarioAsync(int idUsuario, int? idUsuarioAtual = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PostDetalheRespostDto>>([]);

        public Task<(PostDetalheRespostDto Post, bool FoiCriadoNovo)> SalvarOuAtualizarDoUsuarioNoJogoAsync(SalvarPostCommandDto command, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<bool> ExcluirAsync(int idPost, int idUsuarioSolicitante, bool podeModerar = false, CancellationToken ct = default) =>
            Task.FromResult(false);

        public Task<ResultadoCurtidaRespostDto> AlternarCurtidaPostAsync(int idPost, int idUsuario, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<ComentarioRespostDto>> ListarComentariosPorPostAsync(int idPost, int? idUsuarioAtual = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ComentarioRespostDto>>([]);

        public Task<ComentarioRespostDto> AdicionarComentarioAsync(CriarComentarioCommandDto command, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<ResultadoCurtidaRespostDto> AlternarCurtidaComentarioAsync(int idComentario, int idUsuario, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<FaixaDistribuicaoFogueirasDto>> ObterDistribuicaoFogueirasJogoAsync(int idJogo, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<FaixaDistribuicaoFogueirasDto>>([]);
    }

    private sealed class FakeSteamService : ISteamStoreService
    {
        public DestaquesSteamDto Destaques { get; set; } = new()
        {
            MaisVendidos = [],
            Lancamentos = [],
            OfertasEspeciais = [],
            EmBreve = []
        };

        public readonly Dictionary<int, DetalhesJogoSteamDto> DetalhesPorAppId = [];
        public readonly List<ItemBuscaSteamDto> ResultadosBusca = [];
        public bool FalharDetalhes { get; set; }

        public Task<IReadOnlyList<ItemBuscaSteamDto>> BuscarJogosAsync(string jogoBuscado, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ItemBuscaSteamDto>>(ResultadosBusca);

        public Task<DestaquesSteamDto> ObterDestaquesAsync(CancellationToken ct = default) =>
            Task.FromResult(Destaques);

        public Task<DetalhesJogoSteamDto?> ObterDetalhesJogoAsync(int steamAppId, CancellationToken ct = default)
        {
            if (FalharDetalhes)
            {
                throw new IntegrationException("Limite temporário de detalhes na Steam.");
            }

            return Task.FromResult(DetalhesPorAppId.TryGetValue(steamAppId, out var d) ? d : null);
        }
    }

    private sealed class FakeCache : ICatalogoCacheService
    {
        public readonly Dictionary<string, object> Armazenamento = [];

        public Task<T?> ObterAsync<T>(string chave, CancellationToken ct = default) =>
            Task.FromResult(Armazenamento.TryGetValue(chave, out var v) && v is T t ? t : default);

        public Task SalvarAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken ct = default)
        {
            if (valor is not null)
            {
                Armazenamento[chave] = valor;
            }

            return Task.CompletedTask;
        }

        public Task RemoverAsync(string chave, CancellationToken ct = default)
        {
            Armazenamento.Remove(chave);
            return Task.CompletedTask;
        }
    }

    private static FakeSteamService CriarSteamComJogosRelevantes()
    {
        var steam = new FakeSteamService
        {
            Destaques = new DestaquesSteamDto
            {
                MaisVendidos =
                [
                    new ItemDestaqueSteamDto
                    {
                        SteamAppId = 1245620,
                        Nome = "Elden Ring",
                        ImagemCapaUrl = "https://steam/1245620.jpg",
                        PrecoOriginalCentavos = 22990,
                        PrecoFinalCentavos = 16093,
                        PercentualDesconto = 30
                    },
                    new ItemDestaqueSteamDto
                    {
                        SteamAppId = 1086940,
                        Nome = "Baldur's Gate 3",
                        ImagemCapaUrl = "https://steam/1086940.jpg",
                        PrecoOriginalCentavos = 19999,
                        PrecoFinalCentavos = 19999,
                        PercentualDesconto = 0
                    }
                ],
                OfertasEspeciais =
                [
                    new ItemDestaqueSteamDto
                    {
                        SteamAppId = 367520,
                        Nome = "Hollow Knight",
                        ImagemCapaUrl = "https://steam/367520.jpg",
                        PrecoOriginalCentavos = 4699,
                        PrecoFinalCentavos = 2349,
                        PercentualDesconto = 50
                    }
                ],
                Lancamentos =
                [
                    new ItemDestaqueSteamDto
                    {
                        SteamAppId = 553850,
                        Nome = "HELLDIVERS 2",
                        ImagemCapaUrl = "https://steam/553850.jpg",
                        PrecoOriginalCentavos = 19950,
                        PrecoFinalCentavos = 19950
                    }
                ],
                EmBreve = []
            }
        };

        steam.DetalhesPorAppId[1245620] = new DetalhesJogoSteamDto
        {
            SteamAppId = 1245620,
            Nome = "Elden Ring",
            DescricaoCurta = "Ergue-te, Maculado, nas Terras Intermédias.",
            ImagemCapaUrl = "https://steam/1245620.jpg",
            Desenvolvedora = "FromSoftware",
            Publicadora = "Bandai Namco",
            Generos = [Generos.Rpg, Generos.Acao],
            AnoLancamento = 2022,
            NotaMetacritic = 96,
            TotalRecomendacoes = 650000,
            PrecoOriginalCentavos = 22990,
            PrecoFinalCentavos = 16093,
            PercentualDesconto = 30
        };

        steam.DetalhesPorAppId[1086940] = new DetalhesJogoSteamDto
        {
            SteamAppId = 1086940,
            Nome = "Baldur's Gate 3",
            DescricaoCurta = "Reúna seu grupo nos Reinos Esquecidos.",
            ImagemCapaUrl = "https://steam/1086940.jpg",
            Desenvolvedora = "Larian Studios",
            Publicadora = "Larian Studios",
            Generos = [Generos.Rpg, Generos.Estrategia],
            AnoLancamento = 2023,
            NotaMetacritic = 96,
            TotalRecomendacoes = 580000,
            PrecoOriginalCentavos = 19999,
            PrecoFinalCentavos = 19999
        };

        steam.DetalhesPorAppId[367520] = new DetalhesJogoSteamDto
        {
            SteamAppId = 367520,
            Nome = "Hollow Knight",
            DescricaoCurta = "Forje seu caminho em Hallownest.",
            ImagemCapaUrl = "https://steam/367520.jpg",
            Desenvolvedora = "Team Cherry",
            Publicadora = "Team Cherry",
            Generos = [Generos.Indie, Generos.Aventura],
            AnoLancamento = 2017,
            NotaMetacritic = 90,
            TotalRecomendacoes = 340000,
            PrecoOriginalCentavos = 4699,
            PrecoFinalCentavos = 2349,
            PercentualDesconto = 50
        };

        steam.DetalhesPorAppId[553850] = new DetalhesJogoSteamDto
        {
            SteamAppId = 553850,
            Nome = "HELLDIVERS 2",
            DescricaoCurta = "Lute pela Superterra em esquadrão cooperativo.",
            ImagemCapaUrl = "https://steam/553850.jpg",
            Desenvolvedora = "Arrowhead Game Studios",
            Publicadora = "PlayStation Publishing",
            Generos = [Generos.Acao, Generos.MassivoMultiplayer],
            AnoLancamento = 2024,
            NotaMetacritic = 82,
            TotalRecomendacoes = 420000,
            PrecoOriginalCentavos = 19950,
            PrecoFinalCentavos = 19950
        };

        return steam;
    }

    [Fact]
    public async Task VitrineHomeEFallback_QuandoBancoVazio_HomeSinalizaVazioEFallbackRetornaJogosRelevantesSemPoluirSql()
    {
        var catalogoRepo = new FakeCatalogoRepo();
        var postRepo = new FakePostRepo();
        var loggerRepo = new FakeLoggerRepo();
        var steam = CriarSteamComJogosRelevantes();
        var cache = new FakeCache();
        var algoritmoFactory = new AlgoritmoCuradoriaFactory(
        [
            new AlgoritmoPopularidadeJogo(),
            new AlgoritmoTendenciaGenero(),
            new AlgoritmoCustoBeneficio()
        ]);

        var vitrineService = new VitrineService(
            catalogoRepo,
            postRepo,
            loggerRepo,
            steam,
            cache,
            algoritmoFactory);

        // 1. Banco vazio -> /vitrine/home não persiste nada no SQL e sinaliza vitrine sem jogos locais
        var homeVazia = await vitrineService.ObterVitrineHomeAsync();
        Assert.Empty(homeVazia.Destaques);
        Assert.Empty(catalogoRepo.Jogos);
        Assert.Equal(0, catalogoRepo.ChamadasSalvar);

        // 2. /vitrine/fallback retorna destaques, Top 10, seções de gênero e slug 'coop' sem poluir o banco SQL
        var fallback = await vitrineService.ObterVitrineFallbackAsync(limite: 12, algoritmo: "popularidade");

        Assert.NotEmpty(fallback.Destaques);
        Assert.Equal(0, catalogoRepo.ChamadasSalvar);
        Assert.Empty(catalogoRepo.Jogos);
        Assert.True(cache.Armazenamento.ContainsKey(ProvedorFallbackSteam.ChavePoolFallback));

        var secaoTop10 = fallback.Secoes.Single(s => s.Tipo == "TOP_10");
        Assert.Equal(4, secaoTop10.Jogos.Count);
        Assert.All(secaoTop10.Jogos, j =>
        {
            Assert.Equal(0, j.IdJogo);
            Assert.Equal(0.0, j.MediaFogueiras);
            Assert.Equal(0, j.TotalAvaliacoes);
        });

        Assert.Contains(fallback.GenerosDisponiveis, g => g.Slug == "rpg");
        Assert.Contains(fallback.GenerosDisponiveis, g => g.Slug == "coop");
        Assert.Contains(fallback.Secoes, s => s.Tipo == "REVIEWS");
    }

    [Fact]
    public async Task ObterJogosAleatoriosAsync_AlternaEntreSteamFallbackHibridoEBancoLocalConformeDisponibilidade()
    {
        var catalogoRepo = new FakeCatalogoRepo();
        var loggerRepo = new FakeLoggerRepo();
        var steam = CriarSteamComJogosRelevantes();
        var cache = new FakeCache();

        var catalogoService = new CatalogoService(
            catalogoRepo,
            loggerRepo,
            steam,
            cache,
            new BuscaCatalogoStrategyFactory([new BuscaPorNomeStrategy(), new BuscaPorAnoStrategy(), new BuscaPorAutorStrategy()]),
            new AlgoritmoCuradoriaFactory([new AlgoritmoPopularidadeJogo(), new AlgoritmoTendenciaGenero(), new AlgoritmoCustoBeneficio()]));

        // Caso 1: Banco vazio -> steam-fallback
        var descobertaSemBanco = await catalogoService.ObterJogosAleatoriosAsync(new ObterJogosAleatoriosRequestDto
        {
            Limite = 4,
            Algoritmo = "popularidade"
        });

        Assert.Equal("steam-fallback", descobertaSemBanco.OrigemDados);
        Assert.Equal(4, descobertaSemBanco.TotalResultados);

        // Caso 2: Apenas 1 jogo no banco, pedindo 4 -> hibrido (sem duplicar SteamAppId)
        catalogoRepo.Jogos.Add(new JogoCatalogoRespostDto
        {
            IdJogo = 10,
            SteamAppId = 1245620,
            Nome = "Elden Ring",
            Descricao = "No banco local",
            Imagem = "https://steam/1245620.jpg",
            Desenvolvedora = "FromSoftware",
            Publicadora = "Bandai Namco",
            Generos = [Generos.Rpg],
            MediaFogueiras = 4.5,
            TotalAvaliacoes = 2
        });

        var descobertaHibrida = await catalogoService.ObterJogosAleatoriosAsync(new ObterJogosAleatoriosRequestDto
        {
            Limite = 4
        });

        Assert.Equal("hibrido", descobertaHibrida.OrigemDados);
        Assert.Equal(4, descobertaHibrida.TotalResultados);
        Assert.Equal(4, descobertaHibrida.Itens.Select(i => i.SteamAppId).Distinct().Count());
        Assert.Contains(descobertaHibrida.Itens, i => i.SteamAppId == 1245620 && i.IdJogo == 10 && i.MediaFogueiras == 4.5);

        // Caso 3: Limite atendido integralmente pelo banco local -> banco-local
        var descobertaLocal = await catalogoService.ObterJogosAleatoriosAsync(new ObterJogosAleatoriosRequestDto
        {
            Limite = 1
        });

        Assert.Equal("banco-local", descobertaLocal.OrigemDados);
        Assert.Single(descobertaLocal.Itens);
        Assert.Equal(10, descobertaLocal.Itens[0].IdJogo);

        // Validação de limites inválidos
        await Assert.ThrowsAsync<ValidationException>(() =>
            catalogoService.ObterJogosAleatoriosAsync(new ObterJogosAleatoriosRequestDto { Limite = 0 }));
    }

    [Fact]
    public async Task ProvedorFallbackSteam_QuandoDetalhesOscilam_UsaResumoDosDestaquesERelevanciaSuperaJogoIrrelevante()
    {
        var catalogoRepo = new FakeCatalogoRepo();
        var steam = CriarSteamComJogosRelevantes();
        steam.FalharDetalhes = true;
        var cache = new FakeCache();

        var poolResiliente = await ProvedorFallbackSteam.ObterPoolAsync(catalogoRepo, steam, cache);
        Assert.Equal(4, poolResiliente.Count);
        Assert.Contains(poolResiliente, p => p.Jogo.SteamAppId == 1245620);

        // Verifica que um clássico aclamado vence consistentemente um item sem relevância, mantendo variação entre relevantes
        var aclamadoA = new JogoCatalogoRespostDto
        {
            IdJogo = 0,
            SteamAppId = 1,
            Nome = "Clássico Aclamado A",
            Descricao = "Sinopse A",
            Imagem = "img",
            Desenvolvedora = "Dev",
            Publicadora = "Pub",
            Generos = [Generos.Rpg],
            NotaMetacritic = 95,
            TotalRecomendacoes = 500000
        };

        var aclamadoB = aclamadoA with { SteamAppId = 2, Nome = "Clássico Aclamado B" };

        var obscuro = new JogoCatalogoRespostDto
        {
            IdJogo = 0,
            SteamAppId = 3,
            Nome = "Item Obscuro",
            Descricao = "Sinopse C",
            Imagem = "img",
            Desenvolvedora = "Dev",
            Publicadora = "Pub",
            Generos = [],
            NotaMetacritic = 40,
            TotalRecomendacoes = 0
        };

        var algoritmo = new AlgoritmoPopularidadeJogo();
        var contexto = new ContextoCuradoria();
        var entradas = new List<(JogoCatalogoRespostDto, double)>
        {
            (aclamadoA, 1.0),
            (aclamadoB, 1.0),
            (obscuro, 0.5)
        };

        var primeirosColocados = new HashSet<int>();
        for (var seed = 1; seed <= 20; seed++)
        {
            var ranking = ProvedorFallbackSteam.OrdenarComRelevanciaEAleatoriedade(
                entradas,
                algoritmo,
                contexto,
                new Random(seed));

            primeirosColocados.Add(ranking[0].SteamAppId);
            Assert.Equal(3, ranking[^1].SteamAppId);
        }

        Assert.Contains(1, primeirosColocados);
        Assert.Contains(2, primeirosColocados);
    }
}

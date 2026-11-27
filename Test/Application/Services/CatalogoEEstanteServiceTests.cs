using Application.Dtos.Catalogo.Requests;
using Application.Dtos.Estantes.Requests;
using Application.Factories;
using Application.Services;
using Application.Strategies.Algoritmos;
using Application.Strategies.Busca;
using Application.Strategies.Estantes;
using Domain.Dtos.Catalogo;
using Domain.Dtos.Catalogo.Commands;
using Domain.Dtos.Catalogo.Respost;
using Domain.Dtos.Estantes.Commands;
using Domain.Dtos.Estantes.Respost;
using Domain.Dtos.Loggers.Commands;
using Domain.Dtos.Loggers.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;

namespace Test.Application.Services;

public class CatalogoEEstanteServiceTests
{
    private sealed class FakeCatalogoRepository : ICatalogoRepository
    {
        public readonly List<JogoCatalogoRespostDto> Jogos = [];
        private int _nextId = 1;

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

        public Task<JogoCatalogoRespostDto> SalvarOuAtualizarAsync(DetalhesJogoSteamDto d, CancellationToken ct = default)
        {
            var indice = Jogos.FindIndex(j => j.SteamAppId == d.SteamAppId);
            if (indice >= 0)
            {
                var atualizado = Jogos[indice] with
                {
                    Nome = d.Nome,
                    SincronizadoEm = DateTime.UtcNow
                };
                Jogos[indice] = atualizado;
                return Task.FromResult(atualizado);
            }

            var novo = new JogoCatalogoRespostDto
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
        public readonly Dictionary<int, FavoritosRespostDto> FavoritosPorUsuario = [];
        public readonly Dictionary<int, PretendeJogarRespostDto> PretendePorUsuario = [];
        private int _nextId = 1;

        public Task<FavoritosRespostDto?> ObterFavoritosPorUsuarioAsync(int idUsuario, CancellationToken ct = default) =>
            Task.FromResult(FavoritosPorUsuario.TryGetValue(idUsuario, out var fav) ? fav : null);

        public Task<PretendeJogarRespostDto?> ObterPretendeJogarPorUsuarioAsync(int idUsuario, CancellationToken ct = default) =>
            Task.FromResult(PretendePorUsuario.TryGetValue(idUsuario, out var pj) ? pj : null);

        public Task<FavoritosRespostDto> SalvarFavoritosAsync(SalvarFavoritosCommandDto command, CancellationToken ct = default)
        {
            var idCatalogo = FavoritosPorUsuario.TryGetValue(command.IdUsuario, out var atual)
                ? atual.IdCatalogo
                : _nextId++;

            var salvo = new FavoritosRespostDto
            {
                IdCatalogo = idCatalogo,
                IdUsuario = command.IdUsuario,
                Jogos = command.Jogos.ToList(),
                JogoDestaquePerfilId = command.JogoDestaquePerfilId,
                ExibirNoTopoPerfil = command.ExibirNoTopoPerfil,
                ObservacaoDestaque = command.ObservacaoDestaque,
                DataAtualizacao = DateTime.UtcNow
            };

            FavoritosPorUsuario[command.IdUsuario] = salvo;
            return Task.FromResult(salvo);
        }

        public Task<PretendeJogarRespostDto> SalvarPretendeJogarAsync(SalvarPretendeJogarCommandDto command, CancellationToken ct = default)
        {
            var idCatalogo = PretendePorUsuario.TryGetValue(command.IdUsuario, out var atual)
                ? atual.IdCatalogo
                : _nextId++;

            var salvo = new PretendeJogarRespostDto
            {
                IdCatalogo = idCatalogo,
                IdUsuario = command.IdUsuario,
                Jogos = command.Jogos.ToList(),
                NivelPrioridade = command.NivelPrioridade,
                NotificarEmPromocao = command.NotificarEmPromocao,
                MetaAnoConclusao = command.MetaAnoConclusao,
                DataAtualizacao = DateTime.UtcNow
            };

            PretendePorUsuario[command.IdUsuario] = salvo;
            return Task.FromResult(salvo);
        }
    }

    private sealed class FakeLoggerRepository : IEngajamentoLoggerRepository
    {
        public readonly Dictionary<Generos, GeneroLoggerRespostDto> GeneroLoggers = [];
        public readonly Dictionary<int, JogoLoggerRespostDto> JogoLoggers = [];
        public readonly Dictionary<int, UsuarioLoggerRespostDto> UsuarioLoggers = [];
        public List<UsuarioPopularComunidadeDto> Populares = [];

        public Task<IReadOnlyList<GeneroLoggerRespostDto>> ObterGeneroLoggersAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<GeneroLoggerRespostDto>>(GeneroLoggers.Values.ToList());

        public Task<JogoLoggerRespostDto?> ObterJogoLoggerAsync(int idJogo, CancellationToken ct = default) =>
            Task.FromResult(JogoLoggers.TryGetValue(idJogo, out var l) ? l : null);

        public Task<IReadOnlyDictionary<int, JogoLoggerRespostDto>> ObterJogoLoggersPorJogosAsync(IEnumerable<int> idsJogos, CancellationToken ct = default)
        {
            var set = idsJogos.ToHashSet();
            IReadOnlyDictionary<int, JogoLoggerRespostDto> dict = JogoLoggers
                .Where(kv => set.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            return Task.FromResult(dict);
        }

        public Task AplicarImpactoEstanteAsync(ImpactoEngajamentoEstanteCommandDto command, CancellationToken ct = default)
        {
            var agora = DateTime.UtcNow;

            var jogoAtual = JogoLoggers.TryGetValue(command.IdJogo, out var jl)
                ? jl
                : new JogoLoggerRespostDto { IdJogo = command.IdJogo };

            JogoLoggers[command.IdJogo] = jogoAtual with
            {
                PresencaFavoritos = AplicarDelta(jogoAtual.PresencaFavoritos, command.DeltaFavoritosJogo),
                PresencaPretendeJogar = AplicarDelta(jogoAtual.PresencaPretendeJogar, command.DeltaPretendeJogarJogo),
                UltimaAtualizacao = agora
            };

            var usuarioAtual = UsuarioLoggers.TryGetValue(command.IdUsuario, out var ul)
                ? ul
                : new UsuarioLoggerRespostDto { IdUsuario = command.IdUsuario };

            UsuarioLoggers[command.IdUsuario] = usuarioAtual with
            {
                PresencaFavoritos = AplicarDelta(usuarioAtual.PresencaFavoritos, command.DeltaFavoritosUsuario),
                UltimaAtualizacao = agora
            };

            foreach (var genero in command.Generos.Distinct())
            {
                var generoAtual = GeneroLoggers.TryGetValue(genero, out var gl)
                    ? gl
                    : new GeneroLoggerRespostDto { Genero = genero };

                GeneroLoggers[genero] = generoAtual with
                {
                    PresencaFavoritos = AplicarDelta(generoAtual.PresencaFavoritos, command.DeltaFavoritosGenero),
                    PresencaPretendeJogar = AplicarDelta(generoAtual.PresencaPretendeJogar, command.DeltaPretendeJogarGenero),
                    UltimaAtualizacao = agora
                };
            }

            return Task.CompletedTask;
        }

        public Task SincronizarAvaliacoesJogoEUsuarioAsync(
            int idJogo,
            int idUsuario,
            IReadOnlyList<Generos> generosJogo,
            int deltaPostsGenero = 0,
            CancellationToken ct = default) => Task.CompletedTask;

        public Task<IReadOnlyList<UsuarioPopularComunidadeDto>> ObterUsuariosPopularesAsync(int limite = 10, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<UsuarioPopularComunidadeDto>>(Populares.Take(limite).ToList());

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

    private sealed class FakeSteamStoreService : ISteamStoreService
    {
        public bool FalharComunicacao { get; set; }
        public readonly Dictionary<int, DetalhesJogoSteamDto> Detalhes = [];

        public Task<IReadOnlyList<ItemBuscaSteamDto>> BuscarJogosAsync(string jogoBuscado, CancellationToken ct = default)
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

        var regraFactory = new RegraEstanteFactory([new RegraEstanteFavoritos(), new RegraEstantePretendeJogar()]);
        var buscaFactory = new BuscaCatalogoStrategyFactory([new BuscaPorNomeStrategy(), new BuscaPorAnoStrategy(), new BuscaPorAutorStrategy()]);
        var algoritmoFactory = new AlgoritmoCuradoriaFactory([new AlgoritmoPopularidadeJogo(), new AlgoritmoTendenciaGenero(), new AlgoritmoCustoBeneficio()]);

        var estanteService = new EstanteService(
            estanteRepo,
            catalogoRepo,
            loggerRepo,
            steamService,
            cacheService,
            regraFactory);

        var catalogoService = new CatalogoService(
            catalogoRepo,
            loggerRepo,
            steamService,
            cacheService,
            buscaFactory,
            algoritmoFactory);

        var estantes = await estanteService.AdicionarJogoAsync(idUsuario: 42, new AlterarJogoEstanteRequestDto
        {
            TipoEstante = "favoritos",
            SteamAppId = 292030
        });
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
        Assert.Equal(2, loggerRepo.GeneroLoggers.Count);

        var tendencias = await catalogoService.ObterTendenciasGenerosAsync();
        Assert.Equal(12, tendencias.Generos.Count);
        var topDois = tendencias.Generos.Take(2).ToList();
        Assert.Contains(topDois, g => g.Genero == "Rpg" && g.Pontuacao > 0 && g.PresencaFavoritos == 1UL);
        Assert.Contains(topDois, g => g.Genero == "Aventura" && g.Pontuacao > 0 && g.PresencaFavoritos == 1UL);

        var buscaPorAutor = await catalogoService.BuscarAsync(new BuscarCatalogoRequestDto
        {
            Tipo = "autor",
            JogoBuscado = "CD PROJEKT",
            Algoritmo = "popularidade"
        });
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

        var regraFactory = new RegraEstanteFactory([new RegraEstanteFavoritos(), new RegraEstantePretendeJogar()]);

        var estanteService = new EstanteService(
            estanteRepo,
            catalogoRepo,
            loggerRepo,
            steamService,
            cacheService,
            regraFactory);

        // 1. Usuário adiciona sem promoção no momento
        await estanteService.AdicionarJogoAsync(idUsuario: 15, new AlterarJogoEstanteRequestDto
        {
            TipoEstante = "pretende-jogar",
            SteamAppId = 1091500
        });

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

        catalogoRepo.Jogos.Add(new JogoCatalogoRespostDto
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
            new BuscaCatalogoStrategyFactory([new BuscaPorNomeStrategy()]),
            new AlgoritmoCuradoriaFactory([new AlgoritmoPopularidadeJogo()]));

        var detalhes = await catalogoService.ObterDetalhesPorSteamAppIdAsync(new ObterDetalhesJogoRequestDto
        {
            SteamAppId = 1245620
        });

        Assert.Equal("ELDEN RING", detalhes.Nome);
        Assert.Equal(2022, detalhes.AnoLancamento);
    }

    [Fact]
    public async Task ObterUsuariosPopularesAsync_ConsideraSeguidoresEBonusDeFotoPerfilNaPontuacaoERanking()
    {
        var catalogoRepo = new FakeCatalogoRepository();
        var loggerRepo = new FakeLoggerRepository();
        var steamService = new FakeSteamStoreService();
        var cacheService = new FakeCacheService();

        loggerRepo.Populares =
        [
            new UsuarioPopularComunidadeDto
            {
                IdUsuario = 1,
                NomeUsuario = "SemFoto",
                PresencaPost = 2,
                CurtidasRecebidas = 5,
                QuantidadeSeguidores = 4,
                PossuiFotoPerfil = false,
                UltimosRelatos = []
            },
            new UsuarioPopularComunidadeDto
            {
                IdUsuario = 2,
                NomeUsuario = "ComFoto",
                PresencaPost = 2,
                CurtidasRecebidas = 5,
                QuantidadeSeguidores = 4,
                PossuiFotoPerfil = true,
                UltimosRelatos = []
            }
        ];

        var catalogoService = new CatalogoService(
            catalogoRepo,
            loggerRepo,
            steamService,
            cacheService,
            new BuscaCatalogoStrategyFactory([new BuscaPorNomeStrategy()]),
            new AlgoritmoCuradoriaFactory([new AlgoritmoPopularidadeJogo()]));

        var populares = await catalogoService.ObterUsuariosPopularesAsync(new ObterUsuariosPopularesRequestDto
        {
            Limite = 10
        });

        Assert.Equal(2, populares.Count);
        Assert.Equal("ComFoto", populares[0].NomeUsuario);
        Assert.True(populares[0].PossuiFotoPerfil);
        Assert.Equal(4UL, populares[0].QuantidadeSeguidores);
        Assert.Equal(populares[1].PontuacaoPopularidade + 10.0, populares[0].PontuacaoPopularidade);
    }
}
